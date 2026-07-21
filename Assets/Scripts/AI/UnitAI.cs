using System;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleUnit))]
[RequireComponent(typeof(NavMeshAgent))]
public class UnitAI : MonoBehaviour
{
    private enum UnitState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    private static readonly int SpeedParameter = Animator.StringToHash("Speed");

    [Header("Targeting")]
    [Min(0.05f)]
    [SerializeField] private float targetSearchInterval = 0.25f;

    [Header("Movement")]
    [Min(0.05f)]
    [SerializeField] private float repathInterval = 0.2f;
    [Min(0f)]
    [SerializeField] private float targetMoveThreshold = 0.25f;
    [Min(0f)]
    [SerializeField] private float rotationSpeed = 10f;

    private BattleUnit unit;
    private NavMeshAgent agent;
    private Animator animator;
    private BattleUnit currentTarget;
    private UnitState currentState;
    private float nextTargetSearchTime;
    private float nextRepathTime;
    private Vector3 lastTargetPosition;

    public event Action<BattleUnit> TargetChanged;

    public BattleUnit CurrentTarget => currentTarget;
    public bool IsInAttackRange { get; private set; }

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (unit.Data == null)
        {
            Debug.LogError($"{name} cannot run UnitAI without Unit Data.", this);
            enabled = false;
            return;
        }

        agent.speed = unit.Data.MovementSpeed;
        agent.stoppingDistance = unit.Data.AttackRange;
        agent.updateRotation = true;

        ChangeState(UnitState.Idle);
        nextTargetSearchTime = Time.time + UnityEngine.Random.Range(0f, targetSearchInterval);
    }

    private void Update()
    {
        if (unit.IsDead)
        {
            ChangeState(UnitState.Dead);
            UpdateAnimation();
            return;
        }

        if (!IsValidTarget(currentTarget))
        {
            SetTarget(null);
        }

        if (currentTarget == null && Time.time >= nextTargetSearchTime)
        {
            FindNearestEnemy();
            nextTargetSearchTime = Time.time + targetSearchInterval;
        }

        if (currentTarget == null)
        {
            ChangeState(UnitState.Idle);
            StopMoving();
            UpdateAnimation();
            return;
        }

        float attackRange = unit.Data.AttackRange;
        float sqrDistance = (currentTarget.transform.position - transform.position).sqrMagnitude;
        IsInAttackRange = sqrDistance <= attackRange * attackRange;

        if (IsInAttackRange)
        {
            ChangeState(UnitState.Attack);
            StopMoving();
            FaceTarget();
        }
        else
        {
            ChangeState(UnitState.Chase);
            ChaseTarget();
        }

        UpdateAnimation();
    }

    private void FindNearestEnemy()
    {
        BattleUnit nearestEnemy = null;
        float nearestSqrDistance = float.MaxValue;

        foreach (BattleUnit candidate in BattleUnit.ActiveUnits)
        {
            if (!IsValidTarget(candidate))
            {
                continue;
            }

            float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestEnemy = candidate;
            }
        }

        SetTarget(nearestEnemy);
    }

    private bool IsValidTarget(BattleUnit candidate)
    {
        return candidate != null &&
               candidate != unit &&
               !candidate.IsDead &&
               candidate.isActiveAndEnabled &&
               candidate.UnitRace != unit.UnitRace;
    }

    private void SetTarget(BattleUnit newTarget)
    {
        if (currentTarget == newTarget)
        {
            return;
        }

        currentTarget = newTarget;
        IsInAttackRange = false;
        nextRepathTime = 0f;
        lastTargetPosition = newTarget != null ? newTarget.transform.position : transform.position;
        TargetChanged?.Invoke(currentTarget);
    }

    private void ChaseTarget()
    {
        if (!agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;

        bool targetMoved =
            (currentTarget.transform.position - lastTargetPosition).sqrMagnitude >=
            targetMoveThreshold * targetMoveThreshold;

        if (Time.time < nextRepathTime && !targetMoved)
        {
            return;
        }

        lastTargetPosition = currentTarget.transform.position;
        nextRepathTime = Time.time + repathInterval;
        agent.SetDestination(lastTargetPosition);
    }

    private void StopMoving()
    {
        if (!agent.isOnNavMesh || agent.isStopped)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    private void FaceTarget()
    {
        Vector3 direction = currentTarget.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    private void ChangeState(UnitState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;

        if (newState == UnitState.Dead)
        {
            IsInAttackRange = false;
            SetTarget(null);
            StopMoving();
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null)
        {
            return;
        }

        float normalizedSpeed = 0f;

        if (!unit.IsDead && agent.isOnNavMesh && !agent.isStopped && agent.speed > 0f)
        {
            normalizedSpeed = Mathf.Clamp01(agent.velocity.magnitude / agent.speed);
        }

        animator.SetFloat(SpeedParameter, normalizedSpeed, 0.1f, Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        BattleUnit selectedUnit = GetComponent<BattleUnit>();

        if (selectedUnit == null || selectedUnit.Data == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, selectedUnit.Data.AttackRange);
    }
}