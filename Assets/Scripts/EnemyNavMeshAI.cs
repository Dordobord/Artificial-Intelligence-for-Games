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
            ChangeState(EnemyStates.Patrol);
        }
        else
        {
            ChangeState(EnemyStates.Standby);
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
        UpdateAnimation();
    }

    private void CheckForPlayer()
    {
        if (player == null) return;
        //CMeasure the distance between the player and enemy.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        //if the enemy is not chasing AND the player is near
        if (currentState != EnemyStates.Chase && distanceToPlayer <= chaseRange)
        {
            ChangeState(EnemyStates.Chase);
        }
        //if the enemy is chasing, but the player is out of range
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
        // store the new state
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
        //this makes the NavMeshAgent prevent from moving
        agent.isStopped = true;
        //Clear the current path
        agent.ResetPath();
        //Clear the timer
        waitTimer = 0f;
    }

    private void UpdateIdle()
    {
        //Add some codes here if you want the AI to do some action while in idle state.
    }

    //Patrol State
    private void EnterPatrol()
    {
        //Allows AI to move
        agent.isStopped = false;
        //Set the speed to walk speed
        agent.speed = walkSpeed;
        //stops the AI on certain distance during patrol
        agent.stoppingDistance = patrolStoppingDis;
        waitTimer = 0f;
        //Move towards the current patrol point
        SetCurrentPatrolDestination();
    }

    private void UpdatePatrol()
    {
        //if the patrol points were removed, change to idle state.
        if (!HasPatrolPoints)
        {
            ChangeState(EnemyStates.Standby);
            return;
        }
        //Keep moving till the AI reached it
        if (!ReachDestination())
        {
            return;
        }
        //if the AI reached the patrol point
        agent.isStopped = true;
        waitTimer += Time.deltaTime;
        //if AI waited enough on the patrol point
        if(waitTimer >= waitTimeAtWaypoint)
        {
            //reset timer
            waitTimer = 0f;
            ChooseNextPatrolPoint();
            //Makes the AI able to move again
            agent.isStopped = false;
            SetCurrentPatrolDestination();
        }
        
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
        
        if (player == null)
        {
            if (HasPatrolPoints)
            {
                ChangeState(EnemyStates.Patrol);
            }
            else
            {
                ChangeState(EnemyStates.Standby);
            }
            return;
        }

        //Chases player when in range, but stops when it reaches the stopping distance.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= chaseStoppingDis)
        {
            //ATTACK STATE ADD HERE
            agent.isStopped = true;
            agent.ResetPath();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

    }

    private bool ReachDestination()
    {
        if (agent.pathPending) return false;

        //Sometimes remaning distance can be infinity, while the path is unknown.
        //If that happens do not countr as reached.
        if (agent.remainingDistance == Mathf.Infinity) return false;
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
        //if there are no patrol points, do nothing.
        if (!HasPatrolPoints) return;
        //if random patrol is enabled and there is more than one patrol point
        if (randomPatrol && patrolPoints.Length > 1)
        {
            //logic for random patrolling
            int nextIndex = patrolIndex;
            while (nextIndex == patrolIndex)
            {
                nextIndex = Random.Range(0, patrolPoints.Length);
            }
            patrolIndex = nextIndex;
        }
        else
        {
            //move to next patrol point
            patrolIndex++;

            if (patrolIndex >= patrolPoints.Length)
            {
                patrolIndex = 0;
            }
        }

    }

    private void UpdateAnimation()
    {
        float animSpeed = 0f;
        bool isMoving = agent.velocity.magnitude > 0.05f && !agent.isStopped;

        if (currentState == EnemyStates.Patrol && isMoving)
        {
            animSpeed = 0.5f;
        }
        else if (currentState == EnemyStates.Chase && isMoving)
        {
            animSpeed = 1f;
        }

        animator.SetFloat(speedParameter, animSpeed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
