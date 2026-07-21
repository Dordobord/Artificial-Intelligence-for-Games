using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject enemyLeaderPrefab;
    [SerializeField] private GameObject playerLeader;

    [Header("Settings")]
    [SerializeField] private int unitCount = 100;
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private float spawnDistance = 3f;
    [SerializeField]private Vector3 mapSize = new Vector3(40f, 0f, 40f);
    [SerializeField]private LayerMask groundMask;

    [Header("Teams")]
    [SerializeField]private Team playerTeam = Team.Red;

    [SerializeField]
    private Team[] enemyTeams =
    {
        Team.Blue,
        Team.Green,
        Team.Yellow,
        Team.Orange,
    };

    private List<Vector3> usedPositions = new List<Vector3>();

    void Start()
    {
        SetupPlayer();
        SpawnUnits();
        SpawnEnemies();
    }

    void SetupPlayer()
    {
        LeaderController player = playerLeader.GetComponent<LeaderController>();

        player.isPlayer = true;
        player.team = playerTeam;
    }

    void SpawnUnits()
    {
        for (int i = 0; i < unitCount; i++)
        {
            Vector3 pos = GetSpawnPosition();

            if (pos == Vector3.zero)
                continue;

            Instantiate(unitPrefab, pos, Quaternion.identity);
        }
    }

    void SpawnEnemies()
    {
        if (enemyTeams.Length == 0)
            return;

        enemyCount = Mathf.Min(enemyCount, enemyTeams.Length);

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 pos = GetSpawnPosition();
            if (pos == Vector3.zero)
                continue;

            GameObject enemy = Instantiate(enemyLeaderPrefab, pos, Quaternion.identity);

            LeaderController leader = enemy.GetComponent<LeaderController>();
            leader.team = enemyTeams[i];
        }
    }

    private Vector3 GetSpawnPosition()
    {
        int spawnAttempts = 0;
        int maxAttempts = unitCount + enemyCount;
        while (spawnAttempts < maxAttempts)
        {
            float x = Random.Range(-mapSize.x, mapSize.x);
            float z = Random.Range(-mapSize.z, mapSize.z);
            Vector3 randomPos = new Vector3(x, 10f, z);

            Ray ray = new Ray(randomPos, Vector3.down);
            RaycastHit hit;

            bool hitGround = Physics.Raycast(ray, out hit, 100f, groundMask, QueryTriggerInteraction.Ignore);
            if (!hitGround)
            {
                spawnAttempts++;
                continue;
            }

            NavMeshHit navHit;
            bool validNavMesh = NavMesh.SamplePosition(hit.point, out navHit, 0.2f, NavMesh.AllAreas);

            if (!validNavMesh)
            {
                spawnAttempts++;
                continue;
            }

            bool tooClose = false;
            foreach (Vector3 pos in usedPositions)
            {
                float distance = Vector3.Distance(navHit.position, pos);
                if (distance < spawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            if (!tooClose)
            {
                usedPositions.Add(navHit.position);
                return navHit.position;
            }
            spawnAttempts++;
        }
        return Vector3.zero;
    }
}