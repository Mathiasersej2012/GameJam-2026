using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Haptics;
using Cinemachine;
using StarterAssets;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SniperGun : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Camera shootCamera;
    [SerializeField] private float shootDistance = 100f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float fireInterval = 1.2f;

    [Header("Audio")]
    [Tooltip("Audio clip to play when shooting.")]
    [SerializeField] private AudioClip shootClip;
    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;
    [Tooltip("Audio clip to play when the rifle is ready to shoot again.")]
    [SerializeField] private AudioClip readyClip;
    [Range(0f, 1f)]
    [SerializeField] private float readyVolume = 1f;

    [Header("Gamepad")]
    [Tooltip("Trigger threshold used to detect a press on gamepad right trigger (R2) for shooting.")]
    [Range(0f, 1f)]
    [SerializeField] private float gamepadTriggerThreshold = 0.5f;

    [Tooltip("Trigger threshold used to consider left trigger (L2) as 'zoom' (right mouse).")]
    [Range(0f, 1f)]
    [SerializeField] private float zoomGamepadThreshold = 0.5f;
    [Tooltip("Gamepad low-frequency rumble strength when shooting.")]
    [Range(0f, 1f)]
    [SerializeField] private float rumbleLowFrequency = 0.4f;
    [Tooltip("Gamepad high-frequency rumble strength when shooting.")]
    [Range(0f, 1f)]
    [SerializeField] private float rumbleHighFrequency = 0.8f;
    [SerializeField] private float rumbleDuration = 0.12f;

    [Header("Zoom")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private float zoomFov = 20f;
    [SerializeField] private GameObject zoomOverlay;
    [Tooltip("Optional explicit noise component for zoom recoil. If empty, it is auto-fetched from zoomCamera.")]
    [SerializeField] private CinemachineBasicMultiChannelPerlin zoomNoise;

    [Header("Zoom Recoil")]
    [SerializeField] private float zoomRecoilDuration = 0.12f;
    [SerializeField] private float zoomRecoilAmplitude = 0.04f;
    [SerializeField] private float zoomRecoilFrequency = 25f;

    [Header("Sensitivity")]
    [Tooltip("Mouse sensitivity (normal, used by FirstPersonController.sensitivity)")]
    [SerializeField] private float mouseSensitivityNormal = 1f;
    [Tooltip("Mouse sensitivity while zoomed")]
    [SerializeField] private float mouseSensitivityZoom = 0.5f;
    [Tooltip("Gamepad rotation sensitivity (normal) applied to FirstPersonController.RotationSpeed")]
    [SerializeField] private float gamepadSensitivityNormal = 1f;
    [Tooltip("Gamepad rotation sensitivity while zoomed")]
    [SerializeField] private float gamepadSensitivityZoom = 0.5f;

    private float _defaultFov;
    private AudioSource _audioSource;

    // Track previous frame right trigger value so we can detect a rising edge (press) for shooting
    private float _prevRightTriggerValue;

    // track zoom state so we only apply sensitivities once when it changes
    private bool _prevIsZooming;

    // reference to FirstPersonController for adjusting RotationSpeed
    private FirstPersonController _firstPersonController;
    private float _nextShootTime;
    private bool _isOnCooldown;
    private Coroutine _zoomRecoilCoroutine;
    private Coroutine _rumbleCoroutine;
    private IDualMotorRumble _activeRumbleDevice;
    private bool _hasWarnedNoRumbleSupport;
    private float _baseNoiseAmplitude;
    private float _baseNoiseFrequency;

    void Start()
    {
        if (zoomCamera != null)
        {
            _defaultFov = zoomCamera.m_Lens.FieldOfView;
            if (zoomNoise == null)
            {
                zoomNoise = zoomCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
            if (zoomNoise != null)
            {
                _baseNoiseAmplitude = zoomNoise.m_AmplitudeGain;
                _baseNoiseFrequency = zoomNoise.m_FrequencyGain;
            }
        }

        if (zoomOverlay != null)
        {
            zoomOverlay.SetActive(false);
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
        _audioSource.playOnAwake = false;

        // cache FirstPersonController if present and apply default sensitivities
        _firstPersonController = FindObjectOfType<FirstPersonController>();
        ApplySensitivities(false);
    }

    void Update()
    {
        HandleShoot();
        HandleZoom();
        HandleCooldownReady();
    }

    private void HandleShoot()
    {
        if (shootCamera == null)
        {
            return;
        }

        if (_isOnCooldown)
        {
            return;
        }

        // Keep mouse left click for desktop, and also support PS5 R2 (right trigger)
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        // Detect gamepad right trigger (R2) rising edge
        bool gamepadPressed = false;
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            float currentRightTrigger = gamepad.rightTrigger.ReadValue();
            // rising edge: current >= threshold and previous < threshold
            if (currentRightTrigger >= gamepadTriggerThreshold && _prevRightTriggerValue < gamepadTriggerThreshold)
            {
                gamepadPressed = true;
            }
            _prevRightTriggerValue = currentRightTrigger;
        }

        if (mousePressed || gamepadPressed)
        {
            PlayShootSound();
            StartCooldown();

            if (IsZoomingInput())
            {
                StartZoomRecoil();
            }

            if (gamepadPressed && gamepad != null)
            {
                StartGamepadRumble(gamepad);
            }

            Ray ray = shootCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, shootDistance))
            {
                if (hit.collider.CompareTag(enemyTag))
                {
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    private void PlayShootSound()
    {
        if (shootClip == null)
        {
            Debug.LogWarning("SniperGun: No shootClip assigned on " + name);
            return;
        }

        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        _audioSource.PlayOneShot(shootClip, shootVolume);
    }

    private void PlayReadySound()
    {
        if (readyClip == null || _audioSource == null)
        {
            return;
        }

        _audioSource.PlayOneShot(readyClip, readyVolume);
    }

    private void StartCooldown()
    {
        _isOnCooldown = true;
        _nextShootTime = Time.time + Mathf.Max(0.01f, fireInterval);
    }

    private void HandleCooldownReady()
    {
        if (!_isOnCooldown)
        {
            return;
        }

        if (Time.time >= _nextShootTime)
        {
            _isOnCooldown = false;
            PlayReadySound();
        }
    }

    private void HandleZoom()
    {
        if (zoomCamera == null)
        {
            return;
        }

        bool isZooming = IsZoomingInput();

        // if zoom state changed, update sensitivities
        if (isZooming != _prevIsZooming)
        {
            ApplySensitivities(isZooming);
            _prevIsZooming = isZooming;
        }

        if (isZooming)
        {
            zoomCamera.m_Lens.FieldOfView = zoomFov;
        }
        else
        {
            zoomCamera.m_Lens.FieldOfView = _defaultFov;
        }

        if (zoomOverlay != null)
        {
            zoomOverlay.SetActive(isZooming);
        }
    }

    // Apply sensitivity values: mouseSensitivity affects static FirstPersonController.sensitivity,
    // gamepad sensitivity adjusts the instance RotationSpeed (affects look from input system path).
    private void ApplySensitivities(bool zoomed)
    {
        FirstPersonController.sensitivity = zoomed ? mouseSensitivityZoom : mouseSensitivityNormal;

        if (_firstPersonController != null)
        {
            _firstPersonController.RotationSpeed = zoomed ? gamepadSensitivityZoom : gamepadSensitivityNormal;
        }
    }

    private bool IsZoomingInput()
    {
        bool isZoomingMouse = Mouse.current != null && Mouse.current.rightButton.isPressed;
        var gamepad = Gamepad.current;
        bool isZoomingGamepad = gamepad != null && gamepad.leftTrigger.ReadValue() >= zoomGamepadThreshold;
        return isZoomingMouse || isZoomingGamepad;
    }

    private void StartZoomRecoil()
    {
        if (_zoomRecoilCoroutine != null)
        {
            StopCoroutine(_zoomRecoilCoroutine);
        }
        _zoomRecoilCoroutine = StartCoroutine(ZoomRecoilRoutine());
    }

    private IEnumerator ZoomRecoilRoutine()
    {
        if (zoomNoise == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, zoomRecoilDuration);
        zoomNoise.m_AmplitudeGain = _baseNoiseAmplitude + zoomRecoilAmplitude;
        zoomNoise.m_FrequencyGain = Mathf.Max(_baseNoiseFrequency, zoomRecoilFrequency);
        yield return new WaitForSeconds(duration);
        zoomNoise.m_AmplitudeGain = _baseNoiseAmplitude;
        zoomNoise.m_FrequencyGain = _baseNoiseFrequency;
        _zoomRecoilCoroutine = null;
    }

    private void StartGamepadRumble(Gamepad gamepad)
    {
        if (gamepad == null)
        {
            return;
        }

        if (gamepad is not IDualMotorRumble dualMotorRumble)
        {
            if (!_hasWarnedNoRumbleSupport)
            {
                Debug.LogWarning("SniperGun: Connected gamepad does not expose dual-motor rumble through Unity Input System.");
                _hasWarnedNoRumbleSupport = true;
            }
            return;
        }

        if (_rumbleCoroutine != null)
        {
            StopCoroutine(_rumbleCoroutine);
        }
        _rumbleCoroutine = StartCoroutine(GamepadRumbleRoutine(dualMotorRumble));
    }

    private IEnumerator GamepadRumbleRoutine(IDualMotorRumble rumbleDevice)
    {
        _activeRumbleDevice = rumbleDevice;
        rumbleDevice.SetMotorSpeeds(rumbleLowFrequency, rumbleHighFrequency);
        yield return new WaitForSeconds(rumbleDuration);
        if (_activeRumbleDevice != null)
        {
            _activeRumbleDevice.SetMotorSpeeds(0f, 0f);
        }
        _activeRumbleDevice = null;
        _rumbleCoroutine = null;
    }

    private void OnDisable()
    {
        StopRumble();
        ResetZoomNoise();
    }

    private void OnDestroy()
    {
        StopRumble();
        ResetZoomNoise();
    }

    private void StopRumble()
    {
        if (_rumbleCoroutine != null)
        {
            StopCoroutine(_rumbleCoroutine);
            _rumbleCoroutine = null;
        }

        if (_activeRumbleDevice != null)
        {
            _activeRumbleDevice.SetMotorSpeeds(0f, 0f);
            _activeRumbleDevice = null;
        }
    }

    private void ResetZoomNoise()
    {
        if (zoomNoise != null)
        {
            zoomNoise.m_AmplitudeGain = _baseNoiseAmplitude;
            zoomNoise.m_FrequencyGain = _baseNoiseFrequency;
        }
    }
}
