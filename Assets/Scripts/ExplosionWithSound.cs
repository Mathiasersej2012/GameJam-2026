using UnityEngine;

public class ExplosionWithSound : MonoBehaviour
{
    [SerializeField] private float destroyAfterSeconds = 2f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionClip;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null && explosionClip != null)
        {
            audioSource.clip = explosionClip;
            audioSource.Play();
        }

        Destroy(gameObject, destroyAfterSeconds);
    }
}
