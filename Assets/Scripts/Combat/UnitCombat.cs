using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleUnit))]
[RequireComponent(typeof(UnitAI))]
public class UnitCombat : MonoBehaviour
{
    private static readonly int MeleeAttackTrigger = Animator.StringToHash("MeleeAttack");
    private static readonly int RangedAttackTrigger = Animator.StringToHash("RangedAttack");
    private static readonly int CastTrigger = Animator.StringToHash("Cast");

    [Header("Attack Timing")]
    [Min(0f)]
    [SerializeField] private float attackImpactDelay = 0.4f;
    [Min(0f)]
    [SerializeField] private float meleeRangeTolerance = 0.35f;

    [Header("Ranged Attack")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private BattleProjectile projectilePrefab;

    private BattleUnit unit;
    private UnitAI unitAI;
    private Animator animator;
    private UnitAbility ability;
    private float nextAttackTime;
    private bool isAttacking;

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
        unitAI = GetComponent<UnitAI>();
        animator = GetComponentInChildren<Animator>();
        ability = GetComponent<UnitAbility>();
    }

    private void Update()
    {
        if (unit.IsDead || isAttacking || Time.time < nextAttackTime)
        {
            return;
        }

        BattleUnit target = unitAI.CurrentTarget;

        if (!unitAI.IsInAttackRange || !IsValidTarget(target))
        {
            return;
        }

        StartCoroutine(PerformAttack(target));
    }

    private IEnumerator PerformAttack(BattleUnit target)
    {
        isAttacking = true;

        float cooldownMultiplier = ability == null
            ? 1f
            : Mathf.Max(0.1f, ability.AttackCooldownMultiplier);

        nextAttackTime = Time.time + unit.Data.AttackCooldown * cooldownMultiplier;
        PlayAttackAnimation();

        if (attackImpactDelay > 0f)
        {
            yield return new WaitForSeconds(attackImpactDelay);
        }

        if (!unit.IsDead && IsValidTarget(target))
        {
            ResolveAttack(target);
        }

        isAttacking = false;
    }

    private void ResolveAttack(BattleUnit target)
    {
        int outgoingDamage = unit.Data.Attack;

        if (ability != null)
        {
            outgoingDamage = ability.ModifyOutgoingDamage(outgoingDamage, target);
        }

        outgoingDamage = Mathf.Max(1, outgoingDamage);

        switch (unit.Data.AttackType)
        {
            case UnitAttackType.Melee:
                ResolveMeleeAttack(target, outgoingDamage);
                break;

            case UnitAttackType.Ranged:
            case UnitAttackType.Support:
                ResolveRangedAttack(target, outgoingDamage);
                break;
        }
    }

    private void ResolveMeleeAttack(BattleUnit target, int outgoingDamage)
    {
        float allowedRange = unit.Data.AttackRange + meleeRangeTolerance;
        float sqrDistance = (target.transform.position - transform.position).sqrMagnitude;

        if (sqrDistance > allowedRange * allowedRange)
        {
            return;
        }

        int damageDealt = target.TakeDamage(outgoingDamage);
        NotifyAttackHit(target, damageDealt);
    }

    private void ResolveRangedAttack(BattleUnit target, int outgoingDamage)
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning($"{name} is missing its projectile setup.", this);
            return;
        }

        BattleProjectile projectile = Instantiate(
            projectilePrefab,
            projectileSpawnPoint.position,
            projectileSpawnPoint.rotation);

        projectile.Initialize(target, outgoingDamage, NotifyAttackHit);
    }

    private void NotifyAttackHit(BattleUnit target, int damageDealt)
    {
        if (damageDealt > 0 && ability != null)
        {
            ability.OnAttackHit(target, damageDealt);
        }
    }

    private void PlayAttackAnimation()
    {
        if (animator == null)
        {
            return;
        }

        switch (unit.Data.AttackType)
        {
            case UnitAttackType.Melee:
                animator.SetTrigger(MeleeAttackTrigger);
                break;

            case UnitAttackType.Ranged:
                animator.SetTrigger(RangedAttackTrigger);
                break;

            case UnitAttackType.Support:
                animator.SetTrigger(CastTrigger);
                break;
        }
    }

    private bool IsValidTarget(BattleUnit target)
    {
        return target != null &&
               !target.IsDead &&
               target.UnitRace != unit.UnitRace;
    }
}