using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class EnemyNavMeshAI : MonoBehaviour
{
    private enum EnemyStates
    {
        Standby,
        Patrol,
        Chase
    }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform[] patrolPoints;

    [Header("Detection")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float loseRange = 16f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float patrolStoppingDis = .2f;
    [SerializeField] private float chaseStoppingDis = 1f;
    [SerializeField] private float waypointReachDist = .5f;
    [SerializeField] private float waitTimeAtWaypoint = 1.5f; //How long the enemy waits at each points
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private bool randomPatrol = false;

    [Header("Animation")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private float animationDampTime = 0.1f; //Makes the animation not look so sudden

    private NavMeshAgent agent;
    private Animator animator;
    private EnemyStates currentState;
    private bool stateInitialized;
    private int patrolIndex;
    private float waitTimer;

    private bool HasPatrolPoints
    {
        get
        {
            return patrolPoints != null && patrolPoints.Length > 0;
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        if (HasPatrolPoints)
        {
            
        }
    }

    void Update()
    {
        CheckForPlayer();

        switch (currentState)
        {
            case EnemyStates.Standby:
                UpdateIdle();
                break;
            case EnemyStates.Patrol:
                UpdatePatrol();
                break;
            case EnemyStates.Chase:
                UpdateChase();
                break;
        }
    }

    private void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (currentState != EnemyStates.Chase && distanceToPlayer <= chaseRange)
        {
            ChangeState(EnemyStates.Chase);
        }
        else if (currentState == EnemyStates.Chase && distanceToPlayer >= loseRange)
        {
            ChangeState(EnemyStates.Patrol);
        }

    }
    //This method changes the enemy state
    private void ChangeState(EnemyStates newState)
    {
        //If the state was already Intiliazed and the enemy is already in this state.
        //do not restart the same state again
        if (stateInitialized && currentState == newState) return;

        stateInitialized = true;
        currentState = newState;

        switch (currentState)
        {
            case EnemyStates.Standby:
                EnterIdle();
                break;
            case EnemyStates.Patrol:
                EnterPatrol();
                break;
            case EnemyStates.Chase:
                EnterChase();
                break;
        }
    }

    //Idle State
    private void EnterIdle()
    {
        agent.isStopped = true;
        agent.ResetPath();
        waitTimer = 0f;
    }

    private void UpdateIdle()
    {
        
    }

    //Patrol State
    private void EnterPatrol()
    {
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = patrolStoppingDis;
        waitTimer = 0f;
    }

    private void UpdatePatrol()
    {
        
    }

    //Chase State

    private void EnterChase()
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = chaseStoppingDis;
        waitTimer = 0f;
    }

    private void UpdateChase()
    {
        
    }

    private bool ReachDestination()
    {
        if (agent.pathPending) return false;

        //Sometimes remaning distance can be infinity, while the path is unknown.
        //If that happens do not countr as reached.
        if (agent.remainingDistance <= Mathf.Infinity) return false;
        //use whichever values is bigger.
        //agents stopping distance or our custom waypoint reach;.
        float reachedDistance = Mathf.Max(agent.stoppingDistance, waypointReachDist);
        //if the remaining distance is less than or equal to the reached distance, we have reached our destination.
        return agent.remainingDistance <= reachedDistance;
    }

    //this method sends the enemy to the current patrol point/
    private void SetCurrentPatrolDestination()
    {
        //if there are no patrol points, do nothing.
        if (!HasPatrolPoints) return;

        Transform point = patrolPoints[patrolIndex];
        //if this patrol point is missing, choose another one.
        if (point == null)
        {
            ChooseNextPatrolPoint();
            point = patrolPoints[patrolIndex];
        }
        //if patrol point is valid, set it as the destination.
        if (point != null)
        {
            agent.SetDestination(point.position);
        }
    }

    //this method chooses the next patrol point
    private void ChooseNextPatrolPoint()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
