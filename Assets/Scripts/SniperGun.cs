using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(AudioSource))]
public class SniperGun : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private Camera shootCamera;
    [SerializeField] private float shootDistance = 100f;
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Audio")]
    [Tooltip("Audio clip to play when left clicking (shot sound).")]
    [SerializeField] private AudioClip shootClip;
    [Range(0f, 1f)]
    [SerializeField] private float shootVolume = 1f;

    [Header("Zoom")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private float zoomFov = 20f;
    [SerializeField] private GameObject zoomOverlay;

    private float _defaultFov;
    private AudioSource _audioSource;

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

        if (Mouse.current.leftButton.wasPressedThisFrame)
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

        bool isZooming = Mouse.current != null && Mouse.current.rightButton.isPressed;
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
}
