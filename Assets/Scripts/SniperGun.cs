using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using Cinemachine;
using StarterAssets;

[RequireComponent(typeof(AudioSource))]
public class SniperGun : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Camera shootCamera;
    [SerializeField] private float shootDistance = 100f;
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Audio")]
    [Tooltip("Audio clip to play when shooting.")]
    [SerializeField] private AudioClip shootClip;
    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;

    [Header("Gamepad")]
    [Tooltip("Trigger threshold used to detect a press on gamepad right trigger (R2) for shooting.")]
    [Range(0f, 1f)]
    [SerializeField] private float gamepadTriggerThreshold = 0.5f;

    [Tooltip("Trigger threshold used to consider left trigger (L2) as 'zoom' (right mouse).")]
    [Range(0f, 1f)]
    [SerializeField] private float zoomGamepadThreshold = 0.5f;

    [Header("Zoom")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private float zoomFov = 20f;
    [SerializeField] private GameObject zoomOverlay;

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

    void Start()
    {
        if (zoomCamera != null)
        {
            _defaultFov = zoomCamera.m_Lens.FieldOfView;
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
    }

    private void HandleShoot()
    {
        if (shootCamera == null)
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

    private void HandleZoom()
    {
        if (zoomCamera == null)
        {
            return;
        }

        // Right mouse button OR gamepad left trigger (L2) held => zoom
        bool isZoomingMouse = Mouse.current != null && Mouse.current.rightButton.isPressed;

        var gamepad = Gamepad.current;
        bool isZoomingGamepad = gamepad != null && gamepad.leftTrigger.ReadValue() >= zoomGamepadThreshold;

        bool isZooming = isZoomingMouse || isZoomingGamepad;

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
}
