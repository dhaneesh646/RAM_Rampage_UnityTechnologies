using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class RamBattleManager : MonoBehaviour
{
    private RamFighter ramA;
    private RamFighter ramB;
    public TMP_Text winnerText;
    public TMP_Text combatLogText;
    public float postBattleDelay = 3f;

    private bool ramAAttackComplete = false;
    private bool ramBAttackComplete = false;
    private Coroutine battleCoroutine;
    [SerializeField] GameObject[] ramPrefabs;
    [SerializeField] GameObject resultpanel;
    [SerializeField] Button replayButton;
    [SerializeField] Button exitButton;


    void Start()
    {
        replayButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }



    private void OnDestroy()
    {
        if (ramA != null) ramA.OnAttackSequenceComplete -= OnRamAAttackComplete;
        if (ramB != null) ramB.OnAttackSequenceComplete -= OnRamBAttackComplete;

        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
        }
    }

    public void StartBattle()
    {
        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
        }
        battleCoroutine = StartCoroutine(BattleLoop());
    }

    public void StopBattle()
    {
        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
            battleCoroutine = null;
        }
    }

    private void OnRamAAttackComplete()
    {
        ramAAttackComplete = true;
    }

    private void OnRamBAttackComplete()
    {
        ramBAttackComplete = true;
    }

    IEnumerator BattleLoop()
    {
        if (ramA != null) ramA.ResetHealth();
        if (ramB != null) ramB.ResetHealth();

        if (winnerText != null) winnerText.text = "";
        if (combatLogText != null) combatLogText.text = "";

        yield return new WaitForSeconds(1f);

        if (ramA == null || ramB == null)
        {
            Debug.LogError("RamFighter references not set in BattleManager!");
            yield break;
        }

        bool ramATurnFirst = Random.value > 0.5f;

        if (combatLogText != null)
        {
            if (ramATurnFirst)
                combatLogText.text += ramA.GetRamName() + " wins the initiative!";
            else
                combatLogText.text += ramB.GetRamName() + " wins the initiative!";
        }

        yield return new WaitForSeconds(1f);

        while (ramA.IsAlive() && ramB.IsAlive())
        {
            if (ramATurnFirst)
            {
                if (combatLogText != null)
                    combatLogText.text += ramA.GetRamName() + " attacks!";

                ramAAttackComplete = false;
                ramA.InitiateAttackSequence();

                yield return new WaitUntil(() => ramAAttackComplete);
                ramAAttackComplete = false;

                if (!ramB.IsAlive()) break;

                if (combatLogText != null)
                    combatLogText.text += ramB.GetRamName() + " attacks!";

                ramBAttackComplete = false;
                ramB.InitiateAttackSequence();

                yield return new WaitUntil(() => ramBAttackComplete);
                ramBAttackComplete = false;
            }
            else
            {
                if (combatLogText != null)
                    combatLogText.text += ramB.GetRamName() + " attacks!";

                ramBAttackComplete = false;
                ramB.InitiateAttackSequence();

                yield return new WaitUntil(() => ramBAttackComplete);
                ramBAttackComplete = false;

                if (!ramA.IsAlive()) break;

                if (combatLogText != null)
                    combatLogText.text += ramA.GetRamName() + " attacks!";

                ramAAttackComplete = false;
                ramA.InitiateAttackSequence();

                yield return new WaitUntil(() => ramAAttackComplete);
                ramAAttackComplete = false;
            }
        }

        if (ramA.IsAlive())
        {
            if (winnerText != null)
                winnerText.text = "Winner: " + ramA.GetRamName();
            if (combatLogText != null)
                combatLogText.text += ramA.GetRamName() + " is victorious!";
        }
        else if (ramB.IsAlive())
        {
            if (winnerText != null)
                winnerText.text = "Winner: " + ramB.GetRamName();
            if (combatLogText != null)
                combatLogText.text += ramB.GetRamName() + " is victorious!";
        }
        else
        {
            if (winnerText != null)
                winnerText.text = "Draw!";
            if (combatLogText != null)
                combatLogText.text += "Both rams fell in battle!";
        }

        yield return new WaitForSeconds(postBattleDelay);

        StartBattle();
    }

    public void ForceNextTurn()
    {
        ramAAttackComplete = true;
        ramBAttackComplete = true;
    }

    public void SetRams(RamFighter newRamA, RamFighter newRamB)
    {
        // Unsubscribe from old rams
        if (ramA != null) ramA.OnAttackSequenceComplete -= OnRamAAttackComplete;
        if (ramB != null) ramB.OnAttackSequenceComplete -= OnRamBAttackComplete;

        // Set new rams
        ramA = newRamA;
        ramB = newRamB;

        // Subscribe to new rams
        if (ramA != null)
        {
            ramA.SetOpponent(ramB);
            ramA.OnAttackSequenceComplete += OnRamAAttackComplete;
        }

        if (ramB != null)
        {
            ramB.SetOpponent(ramA);
            ramB.OnAttackSequenceComplete += OnRamBAttackComplete;
        }
    
    }
}

