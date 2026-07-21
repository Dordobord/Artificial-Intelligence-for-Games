using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BattleProjectile : MonoBehaviour
{
    [Header("Movement")]
    [Min(0.1f)]
    [SerializeField] private float speed = 15f;
    [Min(0f)]
    [SerializeField] private float arcHeight = 2f;
    [Min(0.1f)]
    [SerializeField] private float maximumLifetime = 5f;
    [SerializeField] private float targetHeightOffset = 1f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem impactVFXPrefab;

    private BattleUnit target;
    private int damage;
    private float remainingLifetime;
    private float flightDuration;
    private float flightTime;
    private Vector3 startPosition;
    private Vector3 previousPosition;
    private Action<BattleUnit, int> hitCallback;
    private bool initialized;

    public void Initialize(
        BattleUnit newTarget,
        int rawDamage,
        Action<BattleUnit, int> onHit = null)
    {
        if (newTarget == null || newTarget.IsDead || rawDamage <= 0)
        {
            Destroy(gameObject);
            return;
        }

        target = newTarget;
        damage = rawDamage;
        hitCallback = onHit;
        remainingLifetime = maximumLifetime;
        startPosition = transform.position;
        previousPosition = startPosition;

        float distance = Vector3.Distance(startPosition, GetTargetPosition());
        flightDuration = Mathf.Max(0.1f, distance / speed);
        flightTime = 0f;
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

        flightTime += Time.deltaTime;
        float progress = Mathf.Clamp01(flightTime / flightDuration);
        Vector3 targetPosition = GetTargetPosition();
        Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, progress);
        newPosition.y += 4f * arcHeight * progress * (1f - progress);

        transform.position = newPosition;

        Vector3 travelDirection = newPosition - previousPosition;

        if (travelDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(travelDirection);
        }

        previousPosition = newPosition;

        if (progress >= 1f)
        {
            HitTarget();
        }
    }

    private Vector3 GetTargetPosition()
    {
        return target.transform.position + Vector3.up * targetHeightOffset;
    }

    private void HitTarget()
    {
        if (impactVFXPrefab != null)
        {
            Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
        }

        if (target != null && !target.IsDead)
        {
            int damageDealt = target.TakeDamage(damage);
            hitCallback?.Invoke(target, damageDealt);
        }

        Destroy(gameObject);
    }
}