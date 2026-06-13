using UnityEngine;
using UnityEngine.AI;

public class LeaderAI : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField]
    private float scanRadius = 30f;

    [Header("Movement")]
    [SerializeField]
    private float fleeDistance = 10f;

    [SerializeField]
    private float wanderRadius = 15f;

    [Header("Performance")]
    [SerializeField]
    private float decisionDelay = 0.5f;

    private NavMeshAgent agent;
    private LeaderController leader;
    private float decisionTimer;
    private Unit currentTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        leader = GetComponent<LeaderController>();
    }

    void Update()
    {
        if (leader == null || leader.isDead) return;

        decisionTimer += Time.deltaTime;

        if (decisionTimer >= decisionDelay)
        {
            decisionTimer = 0f;
            Decide();
        }

        if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            agent.ResetPath();
        }
    }

    void Decide()
    {
        if (AnalyzeEnemy())
            return;

        if (FollowCurrentTarget())
            return;

        if (FindNewTarget())
            return;

        Roam();
    }

    public bool AnalyzeEnemy()
    {
        LeaderController[] leaders = FindObjectsByType<LeaderController>(FindObjectsSortMode.None);

        foreach (LeaderController other in leaders)
        {
            if (!IsValidEnemy(other))
                continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance > scanRadius)
                continue;

            //weaker power? attack them
            if (leader.totalPower > other.totalPower)
            {
                if (!HasValidPath(other.transform.position))
                    continue;

                currentTarget = null;

                agent.SetDestination(other.transform.position);

                return true;
            }

            //stronger enemy? go away
            if (other.totalPower > leader.totalPower)
            {
                Vector3 direction = (transform.position - other.transform.position).normalized;
                Vector3 fleePos = transform.position + direction * fleeDistance;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(fleePos, out hit, fleeDistance, NavMesh.AllAreas))
                {
                    currentTarget = null;
                    agent.SetDestination(hit.position);
                    return true;
                }
            }
        }
        return false;
    }

    public bool FollowCurrentTarget()
    {
        if (currentTarget == null)
            return false;

        if (currentTarget.leader != null)
        {
            currentTarget = null;
            return false;
        }

        if (!HasValidPath(currentTarget.transform.position))
        {
            currentTarget = null;
            return false;
        }

        agent.SetDestination(currentTarget.transform.position);
        return true;
    }

    public bool FindNewTarget()
    {
        Unit[] pawns = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        Unit closestPawn = null;

        float closestDistance = Mathf.Infinity;
        foreach (Unit pawn in pawns)
        {
            if (pawn.leader != null)
                continue;

            float distance = Vector3.Distance(transform.position,pawn.transform.position);

            if (distance > scanRadius)
                continue;

            if (!HasValidPath(pawn.transform.position))
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPawn = pawn;
            }
        }

        if (closestPawn == null)
            return false;

        currentTarget = closestPawn;

        agent.SetDestination(closestPawn.transform.position);
        return true;
    }

    public void Roam()
    {
        Vector3 randomPos = transform.position + Random.insideUnitSphere * wanderRadius;
        randomPos.y = 0f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPos, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    public bool IsValidEnemy(LeaderController other)
    {
        if (other == null)
            return false;

        if (other == leader)
            return false;

        if (other.isDead)
            return false;

        if (other.team == leader.team)
            return false;

        return true;
    }

    public bool HasValidPath(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();

        bool validPath = agent.CalculatePath(target, path);

        if (!validPath)
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}