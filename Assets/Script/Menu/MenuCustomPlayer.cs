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

    // PlayerPrefs keys
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string PLAYER_SKIN_KEY = "PlayerSkinIndex";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadPlayerData();
    }

    private void LoadPlayerData()
    {
        // =========================
        // LOAD PLAYER NAME
        // =========================

        PlayerName = PlayerPrefs.GetString(
            PLAYER_NAME_KEY,
            ""
        );

        playerNameInputField.text = PlayerName;


        // =========================
        // LOAD SKIN INDEX
        // =========================

        currentPrefabIndex = PlayerPrefs.GetInt(
            PLAYER_SKIN_KEY,
            0
        );

        // Đảm bảo index không bị lỗi
        if (clonePrefabs.Length > 0)
        {
            currentPrefabIndex = Mathf.Clamp(
                currentPrefabIndex,
                0,
                clonePrefabs.Length - 1
            );

            // Tắt toàn bộ prefab trước
            for (int i = 0; i < clonePrefabs.Length; i++)
            {
                clonePrefabs[i].SetActive(false);
            }

            // Chọn skin đã lưu
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

    private void OnDisable()
    {
        PlayerPrefs.SetInt(
            PLAYER_SKIN_KEY,
            currentPrefabIndex
        );
        PlayerName = playerNameInputField.text;

        // Lưu tên
        PlayerPrefs.SetString(
            PLAYER_NAME_KEY,
            PlayerName
        );
        PlayerPrefs.Save();
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