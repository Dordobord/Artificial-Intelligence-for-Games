using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PedestrianAI : MonoBehaviour
{
    private NavMeshAgent agent;

    public StopLight trafficLight;

    [Header("Crossing Points")]
    public Transform waitPoint;
    public Transform crossPoint;

    private bool hasCrossed = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.SetDestination(waitPoint.position);
    }

    private void Update()
    {
        if (trafficLight == null) return;

        if (!trafficLight.isGreen)
        {
            agent.isStopped = false;

            if (!hasCrossed)
            {
                agent.SetDestination(crossPoint.position);
                hasCrossed = true;
            }
        }
        else
        {
            agent.isStopped = false;

            if (hasCrossed)
            {
                agent.SetDestination(waitPoint.position);
                hasCrossed = false;
            }
        }
    }
}