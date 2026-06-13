using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeaderController : MonoBehaviour
{
    public Team team;
    public Color teamColor;
    public bool isPlayer;

    public List<Unit> followers = new List<Unit>();

    [Header("Power")]
    public int totalPower;

    [HideInInspector]
    public bool isDead;

    private NavMeshAgent agent;
    private Collider col;
    private Renderer[] colorRender;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<Collider>();
        colorRender = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        ApplyTeam();
        UpdatePower();
    }

    public void ApplyTeam()
    {
        switch (team)
        {
            case Team.Red:
                teamColor = Color.red;
                break;

            case Team.Blue:
                teamColor = Color.blue;
                break;

            case Team.Green:
                teamColor = Color.green;
                break;

            case Team.Yellow:
                teamColor = Color.yellow;
                break;

            case Team.Orange:
                teamColor = new Color(1f, 0.5f, 0f);
                break;
        }

        foreach (Renderer rend in colorRender)
        {
            rend.material.color = teamColor;
        }
    }

    public void UpdatePower()
    {
        totalPower = followers.Count + 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        Unit unit = other.GetComponent<Unit>();

        if (unit != null)
        {
            if (unit.leader == null)
            {
                AddUnit(unit);
            }
            return;
        }

        LeaderController enemy =
            other.GetComponent<LeaderController>();

        if (enemy == null) return;

        if (enemy == this) return;

        Combat(enemy);
    }

    public void AddUnit(Unit unit)
    {
        if (followers.Contains(unit)) return;

        unit.leader = this;

        followers.Add(unit);

        if (AudioManager.main != null)
        {
            AudioManager.main.PlayAddUnit();
        }

        UpdatePower();

        Renderer[] unitRends = unit.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in unitRends)
        {
            rend.material.color = teamColor;
        }
    }

    public void Combat(LeaderController enemy)
    {
        if (enemy == null) return;

        if (enemy.isDead || isDead)
            return;

        if (enemy.team == team)
            return;

        if (enemy.totalPower == totalPower)
            return;

        if (totalPower > enemy.totalPower)
        {
            if (AudioManager.main != null)
            {
                AudioManager.main.PlayLeaderKill();
            }

            enemy.DestroyTeam();
        }
        else
        {
            if (AudioManager.main != null)
            {
                AudioManager.main.PlayLeaderDeath();
            }

            DestroyTeam();
        }
    }

    public void DestroyTeam()
    {
        if (isDead) return;

        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        LeaderAI leaderAI =
            GetComponent<LeaderAI>();

        if (leaderAI != null)
        {
            leaderAI.enabled = false;
        }

        foreach (Unit unit in followers)
        {
            if (unit != null)
            {
                Destroy(unit.gameObject);
            }
        }

        if (isPlayer)
        {
            CameraFollow cam =
                Camera.main.GetComponent<CameraFollow>();

            if (cam != null)
            {
                LeaderController[] leaders =  FindObjectsByType<LeaderController>(FindObjectsSortMode.None);

                foreach (LeaderController otherLeader in leaders)
                {
                    if (otherLeader == this) continue;

                    if (otherLeader.isDead) continue;

                    cam.SetTarget(otherLeader.transform);
                    break;
                }
            }

            UIGame.main.GameOver();
        }
        else
        {
            UIGame.main.Show("TEAM " +team.ToString().ToUpper() + " ELIMINATED");
        }
        CheckWinCondition();

        Destroy(gameObject);
    }

    private void CheckWinCondition()
    {
        LeaderController[] leaders = FindObjectsByType<LeaderController>(FindObjectsSortMode.None);

        int aliveCount = 0;
        LeaderController lastLeader = null;

        foreach (LeaderController leader in leaders)
        {
            if (leader.isDead) continue;

            aliveCount++;
            lastLeader = leader;
        }

        if (aliveCount == 1 && lastLeader != null && lastLeader.isPlayer)
        {
            UIGame.main.Victory();
        }
    }
}