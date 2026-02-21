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

    void Start()
    {
        if (target != null)
        {
            // Set destination once at start (no continuous destination updates)
            destination = target.position;
            destinationSet = true;
        }
        else
        {
            Debug.LogWarning("Helicopter: No target assigned on " + name + ". Destination not set.");
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
}
