using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public LeaderController leader;
    private NavMeshAgent agent;

    [Header("Wander")]
    [SerializeField]private float wanderRadius = 10f;
    [SerializeField]private float wanderTimer = 3f;
    [SerializeField]private float followTimer;
    [SerializeField]private float followInterval;

    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    void Update()
    {
        if (leader != null)
        {
            followTimer += Time.deltaTime;
            if (followTimer >= followInterval)
            {
                followTimer = 0f;

                Vector3 followPos = leader.transform.position + Random.insideUnitSphere * 2f;
                followPos.y = 0;

                agent.SetDestination(followPos);
            }
            return;
        }

        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            timer = 0f;
            Vector3 randomPos = RandomPoint(transform.position, wanderRadius);
            agent.SetDestination(randomPos);
        }
    }

    public Vector3 RandomPoint(Vector3 center, float radius)
    {
        Vector3 randomPos =center + Random.insideUnitSphere * radius;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, radius, NavMesh.AllAreas);
        return hit.position;
    }
}