using UnityEngine;

[RequireComponent(typeof(BattleUnit))]
public abstract class UnitAbility : MonoBehaviour
{
    protected BattleUnit Unit { get; private set; }

    public virtual float AttackCooldownMultiplier => 1f;

    protected virtual void Awake()
    {
        Unit = GetComponent<BattleUnit>();
    }

    public virtual int ModifyOutgoingDamage(int baseDamage, BattleUnit target)
    {
        return baseDamage;
    }

    public virtual void OnAttackHit(BattleUnit target, int damageDealt)
    {
    }
}