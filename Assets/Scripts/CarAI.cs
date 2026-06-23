using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarAI : MonoBehaviour
{
    private NavMeshAgent carAgent;

    [Header("Route")]
    public Transform[] destination;
    public int currentWaypointIndex = 0;

    [Header("Traffic Light")]
    public StopLight trafficLight;
    public Transform stopPoint;
    public float stopDistance = 2f;

    [Header("Speed")]
    public float normalSpeed = 5f;
    public float slowSpeed = 2f;
    public float dist;

    [Header("Raycast Car Detection")]
    public Transform rayOrigin;
    public float rayDistance = 4f;
    public LayerMask carLayer;
    public bool carInFront = false;

    private void Start()
    {
        carAgent = GetComponent<NavMeshAgent>();
        carAgent.speed = normalSpeed;

        if (destination.Length > 0)
        {
            MoveToCurrentWaypoint();
        }
    }

    private void Update()
    {
        CheckCarInFront();

        if (carInFront)
        {
            carAgent.isStopped = true;
            return;
        }

        FollowTrafficLight();

        if (carAgent.pathPending)
            return;

        if (carAgent.remainingDistance <= carAgent.stoppingDistance)
        {
            GoToNextWaypoint();
        }
    }

    private void FollowTrafficLight()
    {
        if (trafficLight == null || stopPoint == null)
            return;

        dist = Vector3.Distance(transform.position, stopPoint.position);

        // Stop at red light
        if (!trafficLight.isGreen && dist <= stopDistance)
        {
            carAgent.isStopped = true;
        }
        // Resume movement when green
        else
        {
            carAgent.isStopped = false;
            carAgent.speed = normalSpeed;
        }
    }

    private void MoveToCurrentWaypoint()
    {
        if (destination.Length == 0)
            return;

        carAgent.SetDestination(destination[currentWaypointIndex].position);
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= destination.Length)
        {
            currentWaypointIndex = 0;
        }

        MoveToCurrentWaypoint();
    }

    private void CheckCarInFront()
    {
        carInFront = false;

        if (rayOrigin == null)
            return;

        Vector3 origin = rayOrigin.position;
        Vector3 direction = transform.forward;

        Debug.DrawRay(origin, direction * rayDistance, Color.yellow);

        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, rayDistance, carLayer))
        {
            carInFront = true;
        }
    }
}