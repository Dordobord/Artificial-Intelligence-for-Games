using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class BattleUnit : MonoBehaviour
{
    private static readonly HashSet<BattleUnit> activeUnits = new();
    private static readonly int DieTrigger = Animator.StringToHash("Die");

    [Header("Unit Setup")]
    [SerializeField] private UnitData unitData;
    [SerializeField] private Race race;

    [Header("Death")]
    [Min(0f)]
    [SerializeField] private float destroyDelay = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private Collider[] unitColliders;
    private int currentHealth;
    private bool initialized;

    public static IEnumerable<BattleUnit> ActiveUnits => activeUnits;

    public event Action<BattleUnit> Died;
    public event Action<int, int> HealthChanged;

    public UnitData Data => unitData;
    public Race UnitRace => race;
    public int CurrentHealth => currentHealth;
    public bool IsDead { get; private set; }

    public float HealthNormalized =>
        unitData == null ? 0f : (float)currentHealth / unitData.MaxHealth;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        unitColliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        Initialize();
    }

    private void OnDisable()
    {
        activeUnits.Remove(this);
    }

    public void Configure(UnitData newData, Race newRace)
    {
        if (initialized)
        {
            Debug.LogWarning(
                $"{name} is already initialized and cannot be reconfigured.",
                this);
            return;
        }

        unitData = newData;
        race = newRace;
    }

    public int TakeDamage(int rawDamage)
    {
        if (!initialized || IsDead || rawDamage <= 0)
        {
            return 0;
        }

        int defenseReduction = Mathf.RoundToInt(unitData.Defense * 0.25f);

        int finalDamage = Mathf.Max(1, rawDamage - defenseReduction);

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        HealthChanged?.Invoke(currentHealth, unitData.MaxHealth);

        if (currentHealth == 0)
        {
            Die();
        }

        return finalDamage;
    }

    public int Heal(int amount)
    {
        if (!initialized || IsDead || amount <= 0)
        {
            return 0;
        }

        int previousHealth = currentHealth;

        currentHealth = Mathf.Min(unitData.MaxHealth, currentHealth + amount);

        int restoredHealth = currentHealth - previousHealth;

        if (restoredHealth > 0)
        {
            HealthChanged?.Invoke(currentHealth, unitData.MaxHealth);
        }

        return restoredHealth;
    }

    private void Initialize()
    {
        if (initialized)
        {
            return;
        }

        if (unitData == null)
        {
            Debug.LogError(
                $"{name} is missing Unit Data.",
                this);

            enabled = false;
            return;
        }

        initialized = true;
        IsDead = false;
        currentHealth = unitData.MaxHealth;

        agent.speed = unitData.MovementSpeed;
        agent.stoppingDistance = unitData.AttackRange;

        activeUnits.Add(this);

        HealthChanged?.Invoke(currentHealth, unitData.MaxHealth);
    }

    private void Die()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        activeUnits.Remove(this);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        foreach (Collider unitCollider in unitColliders)
        {
            unitCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetTrigger(DieTrigger);
        }

        Died?.Invoke(this);
        Destroy(gameObject, destroyDelay);
    }

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }
}