using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class BattleManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text resultText;

    [Header("Battle Check")]
    [Min(0.05f)]
    [SerializeField] private float checkInterval = 0.25f;

    private float nextCheckTime;
    private bool hasSeenHumans;
    private bool hasSeenMonsters;

    public event Action<Race?> BattleFinished;

    public bool HasBattleEnded { get; private set; }
    public int LivingHumans { get; private set; }
    public int LivingMonsters { get; private set; }

    private void Start()
    {
        if (resultText != null)
        {
            resultText.gameObject.SetActive(false);
        }

        nextCheckTime = Time.time + checkInterval;
    }

    private void Update()
    {
        if (HasBattleEnded || Time.time < nextCheckTime)
        {
            return;
        }

        nextCheckTime = Time.time + checkInterval;
        CountLivingUnits();

        hasSeenHumans |= LivingHumans > 0;
        hasSeenMonsters |= LivingMonsters > 0;

        if (!hasSeenHumans || !hasSeenMonsters)
        {
            return;
        }

        CheckForWinner();
    }

    private void CountLivingUnits()
    {
        LivingHumans = 0;
        LivingMonsters = 0;

        foreach (BattleUnit unit in BattleUnit.ActiveUnits)
        {
            if (unit == null || unit.IsDead)
            {
                continue;
            }

            if (unit.UnitRace == Race.Human)
            {
                LivingHumans++;
            }
            else if (unit.UnitRace == Race.Monster)
            {
                LivingMonsters++;
            }
        }
    }

    private void CheckForWinner()
    {
        if (LivingHumans > 0 && LivingMonsters > 0)
        {
            return;
        }

        HasBattleEnded = true;

        if (LivingHumans == 0 && LivingMonsters == 0)
        {
            ShowResult("Draw!");
            BattleFinished?.Invoke(null);
        }
        else if (LivingHumans > 0)
        {
            ShowResult("Humans Win!");
            BattleFinished?.Invoke(Race.Human);
        }
        else
        {
            ShowResult("Monsters Win!");
            BattleFinished?.Invoke(Race.Monster);
        }
    }

    private void ShowResult(string message)
    {
        Debug.Log(message, this);

        if (resultText == null)
        {
            return;
        }

        resultText.text = message;
        resultText.gameObject.SetActive(true);
    }
}