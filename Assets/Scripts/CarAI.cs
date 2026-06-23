using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CarAI : MonoBehaviour
{
    private NavMeshAgent carAgent;

    [Header("Route")]
    public Transform[] startRoute;
    public int currentWaypointIndex = 0;
    public Transform[] currentRoute;
    public Transform[] route_1;
    public Transform[] route_2;
    public Transform[] route_3;
    public bool routeChosen = false;    

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

        currentRoute = startRoute;
        currentWaypointIndex = 0;
        MoveToCurrentWaypoint();
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
        if (currentRoute.Length == 0 || currentRoute == null)  return;

        carAgent.SetDestination(currentRoute[currentWaypointIndex].position);
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= currentRoute.Length)
        {
            if (!routeChosen)
            {
                ChooseRandomRoute();
            }
            else
            {
                currentWaypointIndex = 0;
                MoveToCurrentWaypoint();
            }
        }

        MoveToCurrentWaypoint();
    }

    public void ChooseRandomRoute()
    {
        routeChosen = true;
        int randomRoute = Random.Range(0, 2);
        if (randomRoute == 0)
        {
            currentRoute = route_1;
        }
        else if (randomRoute == 1)
        {
            currentRoute = route_2;
        }
        else
        {
            currentRoute = route_3;
        }

        currentWaypointIndex = 0;
        MoveToCurrentWaypoint();
    }

    private void CheckCarInFront()
    {
        carInFront = false;

        if (rayOrigin == null) return;

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