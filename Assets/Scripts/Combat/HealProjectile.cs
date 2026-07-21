using UnityEngine;

[DisallowMultipleComponent]
public class HealProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Min(0.1f)]
    [SerializeField] private float speed = 12f;
    [Min(0.01f)]
    [SerializeField] private float hitDistance = 0.2f;
    [Min(0.1f)]
    [SerializeField] private float maximumLifetime = 5f;
    [SerializeField] private float targetHeightOffset = 1f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem impactVFXPrefab;

    private BattleUnit target;
    private int healAmount;
    private float remainingLifetime;
    private bool initialized;

    public void Initialize(BattleUnit newTarget, int amount)
    {
        if (newTarget == null || newTarget.IsDead || amount <= 0)
        {
            Destroy(gameObject);
            return;
        }

        target = newTarget;
        healAmount = amount;
        remainingLifetime = maximumLifetime;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            Destroy(gameObject);
            return;
        }

        remainingLifetime -= Time.deltaTime;

        if (remainingLifetime <= 0f || target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position + Vector3.up * targetHeightOffset;
        Vector3 direction = targetPosition - transform.position;
        float travelDistance = speed * Time.deltaTime;
        float contactDistance = Mathf.Max(hitDistance, travelDistance);

        if (direction.sqrMagnitude <= contactDistance * contactDistance)
        {
            HealTarget(targetPosition);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            travelDistance);

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void HealTarget(Vector3 targetPosition)
    {
        int restoredHealth = target.Heal(healAmount);

        if (restoredHealth > 0 && impactVFXPrefab != null)
        {
            Instantiate(
                impactVFXPrefab,
                targetPosition,
                Quaternion.identity,
                target.transform);
        }

        Destroy(gameObject);
    }
}