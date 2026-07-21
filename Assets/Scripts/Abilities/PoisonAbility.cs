using UnityEngine;

public class PoisonAbility : UnitAbility
{
    [Range(0f, 1f)]
    [SerializeField] private float poisonChance = 0.25f;
    [Min(1)]
    [SerializeField] private int damagePerTick = 3;
    [Min(0.1f)]
    [SerializeField] private float tickInterval = 1f;
    [Min(0.1f)]
    [SerializeField] private float duration = 4f;

    public override void OnAttackHit(BattleUnit target, int damageDealt)
    {
        if (target == null || target.IsDead || Random.value > poisonChance)
        {
            return;
        }

        PoisonStatus poison = target.GetComponent<PoisonStatus>();

        if (poison == null)
        {
            poison = target.gameObject.AddComponent<PoisonStatus>();
        }

        poison.Apply(damagePerTick, tickInterval, duration);
    }
}