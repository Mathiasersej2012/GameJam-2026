using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using StarterAssets;

[RequireComponent(typeof(AudioSource))]
public class SniperGun : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Camera shootCamera;
    [SerializeField] private float shootDistance = 100f;
    [SerializeField] private string enemyTag = "Enemy";
    [Tooltip("Seconds between each shot.")]
    [SerializeField] private float fireInterval = 1.5f;

    [Header("Audio")]
    [Tooltip("Audio clip to play when left clicking (shot sound).")]
    [SerializeField] private AudioClip shootClip;
    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;
    [Tooltip("Audio clip to play when the rifle is ready to fire again.")]
    [SerializeField] private AudioClip readyClip;
    [Range(0f, 1f)]
    [SerializeField] private float readyVolume = 1f;

    [Header("Zoom")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private float zoomFov = 20f;
    [SerializeField] private GameObject zoomOverlay;
    [Tooltip("Look sensitivity multiplier while zooming (right click held).")]
    [Range(0.05f, 1f)]
    [SerializeField] private float zoomSensitivityMultiplier = 0.35f;
    [Tooltip("Optional explicit reference. If empty, the script finds it at runtime.")]
    [SerializeField] private FirstPersonController lookController;

    [Header("Zoom Recoil")]
    [Tooltip("Camera shake strength when firing while zooming.")]
    [SerializeField] private float recoilAmplitude = 1.2f;
    [Tooltip("Camera shake frequency when firing while zooming.")]
    [SerializeField] private float recoilFrequency = 3.5f;
    [Tooltip("How quickly the recoil shake fades out.")]
    [SerializeField] private float recoilRecoverSpeed = 10f;

    private float _defaultFov;
    private float _defaultRotationSpeed;
    private AudioSource _audioSource;
    private CinemachineBasicMultiChannelPerlin _zoomNoise;
    private float _nextShootTime;
    private bool _isZooming;

    void Start()
    {
        if (zoomCamera != null)
        {
            _defaultFov = zoomCamera.m_Lens.FieldOfView;
            _zoomNoise = zoomCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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

        if (lookController == null)
        {
            lookController = FindFirstObjectByType<FirstPersonController>();
        }

        if (lookController != null)
        {
            _defaultRotationSpeed = lookController.RotationSpeed;
        }
        _nextShootTime = -1f;

        if (_zoomNoise != null)
        {
            _zoomNoise.m_AmplitudeGain = 0f;
            _zoomNoise.m_FrequencyGain = 0f;
        }
    }

    void Update()
    {
        HandleZoom();
        UpdateRecoil();
        HandleShoot();
    }

    private void HandleShoot()
    {
        if (shootCamera == null || Mouse.current == null)
        {
            return;
        }

        if (_nextShootTime >= 0f && Time.time >= _nextShootTime)
        {
            PlayReadySound();
            _nextShootTime = -1f;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_nextShootTime > Time.time)
            {
                return;
            }

            PlayShootSound();
            _nextShootTime = Time.time + fireInterval;

            if (_isZooming)
            {
                TriggerZoomRecoil();
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

    private void HandleZoom()
    {
        _isZooming = Mouse.current != null && Mouse.current.rightButton.isPressed;
        if (zoomCamera != null && _isZooming)
        {
            zoomCamera.m_Lens.FieldOfView = zoomFov;
        }
        else if (zoomCamera != null)
        {
            zoomCamera.m_Lens.FieldOfView = _defaultFov;
        }

        if (zoomOverlay != null)
        {
            zoomOverlay.SetActive(_isZooming);
        }

        if (lookController != null)
        {
            lookController.RotationSpeed = _isZooming
                ? _defaultRotationSpeed * zoomSensitivityMultiplier
                : _defaultRotationSpeed;
        }
    }

    private void TriggerZoomRecoil()
    {
        if (_zoomNoise == null)
        {
            return;
        }

        _zoomNoise.m_AmplitudeGain = recoilAmplitude;
        _zoomNoise.m_FrequencyGain = recoilFrequency;
    }

    private void UpdateRecoil()
    {
        if (_zoomNoise == null)
        {
            return;
        }

        _zoomNoise.m_AmplitudeGain = Mathf.MoveTowards(
            _zoomNoise.m_AmplitudeGain,
            0f,
            recoilRecoverSpeed * Time.deltaTime);

        _zoomNoise.m_FrequencyGain = Mathf.MoveTowards(
            _zoomNoise.m_FrequencyGain,
            0f,
            recoilRecoverSpeed * Time.deltaTime);
    }

    private void OnDisable()
    {
        if (lookController != null)
        {
            lookController.RotationSpeed = _defaultRotationSpeed;
        }

        if (_zoomNoise != null)
        {
            _zoomNoise.m_AmplitudeGain = 0f;
            _zoomNoise.m_FrequencyGain = 0f;
        }
    }
}
