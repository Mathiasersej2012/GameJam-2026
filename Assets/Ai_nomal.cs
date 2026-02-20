using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Ai_nomal : MonoBehaviour
{
    public Transform target;
    public float stoppingDistance = 0.5f;

    private NavMeshAgent agent;

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
}
