using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Ai_nomal : MonoBehaviour
{
    public Transform target;
    public float stoppingDistance = 0.5f;
    public GameObject explosionPrefab;
    public int towerDamage = 1;

    private NavMeshAgent agent;
    private TowerHealth towerHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent missing on " + name);
            enabled = false;
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        towerHealth = FindFirstObjectByType<TowerHealth>();

        if (towerHealth == null)
            Debug.LogWarning("Ai_nomal: Could not find TowerHealth in Start().");

        if (target != null)
        {
            // Set destination once at start of the game
            agent.SetDestination(target.position);
        }
        else
        {
            Debug.LogWarning("Ai_nomal: No target assigned on " + name + ". Destination not set.");
        }

        // Disable the component so it doesn't update destination anymore.
        // Public methods can still be called from other scripts if you need to change target later.
        enabled = false;
    }

    // Call this at runtime to change destination once (will set destination and disable again).
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        target = newTarget;
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.SetDestination(target.position);

        // Re-disable to keep behavior "set once"
        enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Tower"))
            return;

        if (towerHealth != null)
            towerHealth.TakeDamage(towerDamage);
        else
            Debug.LogWarning("Ai_nomal: Tower was hit but cached TowerHealth is missing.");

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        else
            Debug.LogWarning("Ai_nomal: No explosionPrefab assigned on " + name + ".");

        Destroy(gameObject);
    }
}
