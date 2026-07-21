using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BattleUnit))]
public class PoisonStatus : MonoBehaviour
{
    private BattleUnit unit;
    private Coroutine poisonRoutine;

    private void Awake()
    {
        unit = GetComponent<BattleUnit>();
    }

    public void Apply(int damagePerTick, float tickInterval, float duration)
    {
        if (unit.IsDead)
        {
            return;
        }

        if (poisonRoutine != null)
        {
            StopCoroutine(poisonRoutine);
        }

        poisonRoutine = StartCoroutine(PoisonRoutine(
            Mathf.Max(1, damagePerTick),
            Mathf.Max(0.1f, tickInterval),
            Mathf.Max(0.1f, duration)));
    }

    private IEnumerator PoisonRoutine(int damagePerTick, float tickInterval, float duration)
    {
        float elapsedTime = 0f;
        WaitForSeconds tickDelay = new(tickInterval);

        while (elapsedTime < duration && !unit.IsDead)
        {
            yield return tickDelay;
            elapsedTime += tickInterval;

            if (!unit.IsDead)
            {
                unit.TakeDamage(damagePerTick);
            }
        }

        poisonRoutine = null;
        Destroy(this);
    }
}