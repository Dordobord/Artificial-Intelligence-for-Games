using UnityEngine;

public enum UnitAttackType
{
    Melee,
    Ranged,
    Support
}

public enum UnitAbilityType
{
    None,
    CriticalStrike,
    Poison,
    Berserk,
    Heal
}

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Auto Battle/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string unitName = "New Unit";

    [TextArea(2, 4)]
    [SerializeField] private string description;

    [SerializeField] private UnitAttackType attackType = UnitAttackType.Melee;
    [SerializeField] private UnitAbilityType abilityType = UnitAbilityType.None;

    [Header("Stats")]
    [Min(1)] [SerializeField] private int maxHealth = 100;
    [Min(0)] [SerializeField] private int attack = 10;
    [Min(0)] [SerializeField] private int defense = 5;
    [Min(0.1f)] [SerializeField] private float movementSpeed = 5f;
    [Min(0.1f)] [SerializeField] private float attackRange = 1f;
    [Min(0.1f)] [SerializeField] private float attackCooldown = 1.2f;

    public string UnitName => unitName;
    public string Description => description;
    public UnitAttackType AttackType => attackType;
    public UnitAbilityType AbilityType => abilityType;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defense => defense;
    public float MovementSpeed => movementSpeed;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        attack = Mathf.Max(0, attack);
        defense = Mathf.Max(0, defense);
        movementSpeed = Mathf.Max(0.1f, movementSpeed);
        attackRange = Mathf.Max(0.1f, attackRange);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);
    }
}