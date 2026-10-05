using TMPro;
using UnityEngine;

public class MenuCustomPlayer : MonoBehaviour
{
    public static MenuCustomPlayer Instance;

    [Header("Character")]
    [SerializeField] private GameObject[] clonePrefabs;

    [Header("Player Name")]
    [SerializeField] private TMP_InputField playerNameInputField;

    [HideInInspector]
    public string nameResourcesPrefabSelected;

    [HideInInspector]
    public string PlayerName;

    private int currentPrefabIndex = 0;

    private void Awake()
    {
            Instance = this;
    }

    private void Start()
    {
        if (clonePrefabs.Length > 0)
        {
            nameResourcesPrefabSelected =
                clonePrefabs[currentPrefabIndex].name;

            clonePrefabs[currentPrefabIndex].SetActive(true);
        }
    }

    public void ChangeSkin(int direct)
    {
        if (clonePrefabs.Length == 0)
            return;

        clonePrefabs[currentPrefabIndex].SetActive(false);

        currentPrefabIndex += direct;

        if (currentPrefabIndex < 0)
            currentPrefabIndex = clonePrefabs.Length - 1;

        if (currentPrefabIndex >= clonePrefabs.Length)
            currentPrefabIndex = 0;

        nameResourcesPrefabSelected =
            clonePrefabs[currentPrefabIndex].name;

        clonePrefabs[currentPrefabIndex].SetActive(true);
    }

    public void UpdatePlayerName()
    {
        PlayerName = playerNameInputField.text;
    }

    public string GetPlayerName()
    {
        return playerNameInputField.text;
    }

    public string GetCharacter()
    {
        return nameResourcesPrefabSelected;
    }
}