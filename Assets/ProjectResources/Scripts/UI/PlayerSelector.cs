using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

public class PlayerSelector : MonoBehaviour
{
    [Header("Player Selection UI")]
    [SerializeField] TMP_Dropdown playerA;
    [SerializeField] TMP_Dropdown playerB;
    [SerializeField] Image previewPlayerA;
    [SerializeField] Image previewPlayerB;
    [SerializeField] Sprite[] playerSprites;
    [SerializeField] Button starGameButton;
    [SerializeField] GameObject selectionPanel;

    [Space(10)]
    [Header("Battle System")]
    [SerializeField] RamBattleManager battleManager;
    [SerializeField] GameObject[] ramPrefabs;
    [SerializeField] Transform spawnPointA;
    [SerializeField] Transform spawnPointB;
    [SerializeField] RuntimeAnimatorController animatorControllerA;
    [SerializeField] RuntimeAnimatorController animatorControllerB;

    [Space(10)]
    [Header("Health UI")]
    [SerializeField] Image healthA;
    [SerializeField] Image healthB;

    [Space(10)]
    [Header("Internal State")]
    private int selectedIndexA = 0;
    private int selectedIndexB = 0;

    void Start()
    {
        InitializeDropdownOptions();

        playerA.onValueChanged.AddListener((index) =>
        {
            selectedIndexA = index;
            UpdatePlayerAPreview(index);
        });

        playerB.onValueChanged.AddListener((index) =>
        {
            selectedIndexB = index;
            UpdatePlayerBPreview(index);
        });

        UpdatePlayerAPreview(playerA.value);
        UpdatePlayerBPreview(playerB.value);

        starGameButton.onClick.AddListener(StartGame);
    }

    void InitializeDropdownOptions()
    {
        playerA.ClearOptions();
        playerB.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (Sprite ramsprite in playerSprites)
        {
            options.Add(new TMP_Dropdown.OptionData(ramsprite.name));
        }


        playerA.AddOptions(options);
        playerB.AddOptions(options);

        if (options.Count > 1)
        {
            playerB.value = 1;
            selectedIndexB = 1;
        }
    }

    void UpdatePlayerAPreview(int index)
    {
        if (previewPlayerA != null && playerSprites.Length > index && index >= 0)
        {
            previewPlayerA.sprite = playerSprites[index];
        }
    }

    void UpdatePlayerBPreview(int index)
    {
        if (previewPlayerB != null && playerSprites.Length > index && index >= 0)
        {
            previewPlayerB.sprite = playerSprites[index];
        }
    }

    void StartGame()
    {
        string nameA = "Ram A";
        string nameB = "Ram B";

        SpawnRamsAndStartBattle(selectedIndexA, selectedIndexB, nameA, nameB);

        if (selectionPanel != null) selectionPanel.SetActive(false);
    }

    void SpawnRamsAndStartBattle(int indexA, int indexB, string nameA, string nameB)
    {
        if (ramPrefabs == null || ramPrefabs.Length == 0)
        {
            Debug.LogError("No ram prefabs assigned!");
            return;
        }

        ClearExistingRams();

        GameObject ramAObj = Instantiate(ramPrefabs[indexA], spawnPointA.position, spawnPointA.rotation);
        GameObject ramBObj = Instantiate(ramPrefabs[indexB], spawnPointB.position, spawnPointB.rotation);

        RamFighter ramA = ramAObj.GetComponent<RamFighter>();
        RamFighter ramB = ramBObj.GetComponent<RamFighter>();

        ramA.InjectDependencies(healthA, ramB, animatorControllerA);
        ramB.InjectDependencies(healthB, ramA, animatorControllerB);

        

        if (ramA != null) ramA.SetRamName(nameA);
        if (ramB != null) ramB.SetRamName(nameB);

        if (battleManager != null)
        {
            battleManager.SetRams(ramA, ramB);
            battleManager.StartBattle();
        }
    }


    

    void ClearExistingRams()
    {
        RamFighter[] existingRams = FindObjectsOfType<RamFighter>();
        foreach (RamFighter ram in existingRams)
        {
            Destroy(ram.gameObject);
        }
    }

    public void ReturnToSelection()
    {
        if (selectionPanel != null) selectionPanel.SetActive(true);

        if (battleManager != null)
        {
            battleManager.StopBattle();
        }
    }
}