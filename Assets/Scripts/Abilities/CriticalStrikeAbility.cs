using UnityEngine;

public class CriticalStrikeAbility : UnitAbility
{
    [Range(0f, 1f)]
    [SerializeField] private float criticalChance = 0.15f;
    [Min(1f)]
    [SerializeField] private float damageMultiplier = 2f;

    public override int ModifyOutgoingDamage(int baseDamage, BattleUnit target)
    {
        if (Random.value > criticalChance)
        {
            return baseDamage;
        }

        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }
}