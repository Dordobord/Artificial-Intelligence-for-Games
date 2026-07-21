using UnityEngine;

public class HealAbility : UnitAbility
{
    private static readonly int CastTrigger = Animator.StringToHash("Cast");

    [Header("Healing")]
    [Min(1)]
    [SerializeField] private int healAmount = 15;
    [Min(0.1f)]
    [SerializeField] private float healRange = 8f;
    [Min(0.1f)]
    [SerializeField] private float healCooldown = 4f;
    [Min(0.05f)]
    [SerializeField] private float healSearchInterval = 0.25f;
    [Range(0.01f, 1f)]
    [SerializeField] private float healBelowHealth = 0.8f;

    [Header("Healing Projectile")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private HealProjectile projectilePrefab;

    private Animator animator;
    private float nextHealTime;
    private float nextHealSearchTime;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        nextHealTime = Time.time + Random.Range(0f, healCooldown);
    }

    private void Update()
    {
        if (Unit.IsDead ||
            Time.time < nextHealTime ||
            Time.time < nextHealSearchTime)
        {
            return;
        }

        nextHealSearchTime = Time.time + healSearchInterval;

        BattleUnit target = FindMostInjuredAlly();

        if (target == null)
        {
            return;
        }

        if (projectileSpawnPoint == null || projectilePrefab == null)
        {
            Debug.LogWarning($"{name} is missing its healing projectile setup.", this);
            nextHealTime = Time.time + healCooldown;
            return;
        }

        nextHealTime = Time.time + healCooldown;

        HealProjectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation);

        projectile.Initialize(target, healAmount);

        if (animator != null)
        {
            animator.SetTrigger(CastTrigger);
        }
    }

    private BattleUnit FindMostInjuredAlly()
    {
        BattleUnit mostInjuredAlly = null;
        float lowestHealth = healBelowHealth;
        float healRangeSqr = healRange * healRange;

        foreach (BattleUnit candidate in BattleUnit.ActiveUnits)
        {
            if (candidate == null ||
                candidate.IsDead ||
                candidate.UnitRace != Unit.UnitRace)
            {
                continue;
            }

            float sqrDistance = (candidate.transform.position - transform.position).sqrMagnitude;

            if (sqrDistance > healRangeSqr || candidate.HealthNormalized >= lowestHealth)
            {
                continue;
            }

            lowestHealth = candidate.HealthNormalized;
            mostInjuredAlly = candidate;
        }

        return mostInjuredAlly;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, healRange);
    }
}