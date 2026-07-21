using UnityEngine;
using UnityEngine.AI;

public class AgentArrivalCheck : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform[] destinationPos;
    public int destinationIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        agent.SetDestination(destinationPos[destinationIndex].position);
        if (HasReachedDestination(agent))
        {
            Debug.Log("I've reached my destination");
            destinationIndex = (destinationIndex + 1) % destinationPos.Length;
        }
    }

    public bool HasReachedDestination(NavMeshAgent _agent)
    {
        if (_agent.remainingDistance > agent.stoppingDistance) //Check Distance
            return false;

        if (_agent.pathPending) //if AI have a path pending
            return false;

        if (_agent.hasPath && _agent.velocity.sqrMagnitude > 0) // check if moving
            return false;

        return true;
    }
}
