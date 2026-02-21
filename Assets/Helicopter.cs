using UnityEngine;

public class Helicopter : MonoBehaviour
{
    [Tooltip("Target the helicopter should move to. Assign in Inspector or set at runtime.")]
    public Transform target;

    [Tooltip("Move speed in units/second.")]
    public float speed = 5f;

    [Tooltip("Distance at which the helicopter will stop moving toward the target.")]
    public float stoppingDistance = 0.5f;

    [Tooltip("If true the helicopter will rotate to face the movement direction.")]
    public bool faceMovementDirection = true;

    [Header("Explosion & Damage")]
    [Tooltip("Prefab to spawn when helicopter hits the tower (can include particles + AudioSource).")]
    public GameObject explosionPrefab;
    [Tooltip("Amount of damage applied to the tower when hit.")]
    public int towerDamage = 1;

    private Vector3 destination;
    private bool destinationSet;

    // Cached reference to the TowerHealth found at start (if any)
    private TowerHealth _cachedTowerHealth;

    void Start()
    {
        // Find the first object in the scene that has a TowerHealth and use its transform as target.
        _cachedTowerHealth = FindObjectOfType<TowerHealth>();
        if (_cachedTowerHealth != null)
        {
            target = _cachedTowerHealth.transform;
        }

        if (target != null)
        {
            // Set destination once at start (no continuous destination updates)
            destination = target.position;
            destinationSet = true;
        }
        else
        {
            Debug.LogWarning("Helicopter: No target assigned or no TowerHealth found in the scene on " + name + ". Destination not set.");
            destinationSet = false;
        }
    }

    void Update()
    {
        if (!destinationSet)
            return;

        // Stop if within stopping distance
        float sqrRem = (destination - transform.position).sqrMagnitude;
        if (sqrRem <= stoppingDistance * stoppingDistance)
        {
            destinationSet = false;
            return;
        }

        // Move toward the saved destination (only the one set at Start or via SetTarget)
        Vector3 next = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

        if (faceMovementDirection)
        {
            Vector3 dir = (next - transform.position);
            if (dir.sqrMagnitude > 1e-6f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
            }
        }

        transform.position = next;
    }

    // Call this at runtime to set a new single destination (will only set once)
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        target = newTarget;
        destination = newTarget.position;
        destinationSet = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Tower"))
            return;

        // Try to get TowerHealth from the hit object or its parents. Fall back to cached one.
        TowerHealth tower = collision.gameObject.GetComponent<TowerHealth>() 
                            ?? collision.gameObject.GetComponentInParent<TowerHealth>() 
                            ?? _cachedTowerHealth;

        if (tower != null)
        {
            tower.TakeDamage(towerDamage);
        }
        else
        {
            Debug.LogWarning("Helicopter: Hit Tower but no TowerHealth found on " + collision.gameObject.name);
        }

        Vector3 spawnPos = collision.contacts != null && collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, spawnPos, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Tower"))
            return;

        TowerHealth tower = other.gameObject.GetComponent<TowerHealth>() 
                            ?? other.GetComponentInParent<TowerHealth>() 
                            ?? _cachedTowerHealth;

        if (tower != null)
        {
            tower.TakeDamage(towerDamage);
        }
        else
        {
            Debug.LogWarning("Helicopter: Trigger hit Tower but no TowerHealth found on " + other.gameObject.name);
        }

        Vector3 spawnPos = other.ClosestPoint(transform.position);
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, spawnPos, Quaternion.identity);

        Destroy(gameObject);
    }
}
