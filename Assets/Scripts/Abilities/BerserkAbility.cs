using UnityEngine;

public class BerserkAbility : UnitAbility
{
    [Range(0.01f, 1f)]
    [SerializeField] private float healthThreshold = 0.2f;
    [Range(0.1f, 1f)]
    [SerializeField] private float cooldownMultiplier = 0.4f;

    public bool IsBerserk => !Unit.IsDead && Unit.HealthNormalized <= healthThreshold;

    public override float AttackCooldownMultiplier => IsBerserk ? cooldownMultiplier : 1f;
}