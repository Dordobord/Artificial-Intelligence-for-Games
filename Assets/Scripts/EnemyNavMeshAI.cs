using UnityEngine;
using UnityEngine.AI;
public class EnemyNavMeshAI : MonoBehaviour
{
    private enum EnemyStates
    {
        Idle,
        Patrol,
        Chase
    }

    [Header("References")]
    [SerializeField] Transform player;
    [SerializeField] Transform[] patrolPoints;

    [Header("Detection")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float loseRange = 16f;

    [Header("Vision")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private float loseSightTime = 3f;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float patrolStoppingDis = .2f;
    [SerializeField] private float chaseStoppingDis = 1f;
    [SerializeField] private float waypointReachDist = .5f;
    [SerializeField] private float waitTimeAtWaypoint = 1.5f; //How long the enemy waits at each points
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private bool randomPatrol = false;

    [Header("Path Recovery")]
    [SerializeField] private float repathDelay = 1f;

    [Header("Animation")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private float animationDampTime = 0.1f; //Makes the animation not look so sudden
    [SerializeField] private string attackTrigger = "Attack";
    [SerializeField] private float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private EnemyStates currentState;
    private bool stateInitialized;
    private int patrolIndex;
    private float waitTimer;
    private float loseSightTimer;
    private float repathTimer;
    private bool isAttacking;
    private float attackTimer;
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

        agent.updateRotation = false;   
    }

    void Start()
    {
        if (HasPatrolPoints)
        {
            ChangeState(EnemyStates.Patrol);
        }
        else
        {
            ChangeState(EnemyStates.Idle);
        }
    }

    void Update()
    {
        CheckForPlayer();
        
        switch (currentState)
        {
            case EnemyStates.Idle:
                UpdateIdle();
                break;
            case EnemyStates.Patrol:
                UpdatePatrol();
                break;
            case EnemyStates.Chase:
                UpdateChase();
                break;
        }

        HandlePathFailure();
        HandleRotation();
        UpdateAnimation();
    }

    private void CheckForPlayer()
    {
        if (player == null) return;
        
        Vector3 directionToPlayer = (player.position - eyePoint.position).normalized;
        //CMeasure the distance between the player and enemy.
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        //if the enemy is not chasing AND the player is near

        if (distanceToPlayer > loseRange)
        {
            loseSightTimer += Time.deltaTime;

            if (loseSightTimer >= loseSightTime)
            {
                if (HasPatrolPoints)
                {
                    ChangeState(EnemyStates.Patrol);
                }
                else
                {
                    ChangeState(EnemyStates.Idle);
                }
            }

            return;
        }

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > fieldOfView * 0.5f)
        {
            loseSightTimer += Time.deltaTime;
            return;
        }

        bool hasLineOfSight = 
            Physics.Raycast(eyePoint.position, directionToPlayer, out RaycastHit hit, chaseRange, obstacleMask,QueryTriggerInteraction.Ignore);

        if (hasLineOfSight)
        {
            if (hit.transform.CompareTag("Player"))
            {
                loseSightTimer = 0f;

                if (currentState != EnemyStates.Chase)
                {
                    ChangeState(EnemyStates.Chase);
                }
            }
            else
            {
                loseSightTimer += Time.deltaTime;
            }
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
            case EnemyStates.Idle:
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
            ChangeState(EnemyStates.Idle);
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
                ChangeState(EnemyStates.Idle);
            }
            return;
        }

        if (isAttacking)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();

            attackTimer += Time.deltaTime;

            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                isAttacking = false;
                agent.isStopped = false;
                agent.SetDestination(player.position);
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

            if (!isAttacking)
            {
                isAttacking = true;
                attackTimer = 0f;
                animator.SetTrigger(attackTrigger);
            }
        }
        else
        {
            isAttacking = false;
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

    }

    private void HandleRotation()
    {
        if (isAttacking) return;

        Vector3 moveDirection = agent.velocity.normalized;
        moveDirection.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
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

    private void HandlePathFailure()
    {
        if (agent.pathPending) return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            repathTimer += Time.deltaTime;

            if (repathTimer >= repathDelay)
            {
                repathTimer = 0f;

                if (currentState == EnemyStates.Chase && player != null)
                {
                    agent.SetDestination(player.position);
                }
                else if (currentState == EnemyStates.Patrol)
                {
                    SetCurrentPatrolDestination();
                }
            }
        }
        else
        {
            repathTimer = 0f;
        }
    }

    private void UpdateAnimation()
    {
        if (isAttacking)
        {
            animator.SetFloat(speedParameter, 0f);
            return;
        }
        float animSpeed = 0f;
        bool isMoving = agent.desiredVelocity.magnitude > 0.05f && !agent.isStopped;

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

        Gizmos.color = Color.blue;

        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * chaseRange);
        Gizmos.DrawRay(transform.position, rightBoundary * chaseRange);
    }
}
