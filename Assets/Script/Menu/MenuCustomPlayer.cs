using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuCustomPlayer : MonoBehaviour
{
    public static MenuCustomPlayer Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

    }
    [HideInInspector] public string nameResourcesPrefabSelected;
    [HideInInspector] public string PlayerName;
    [SerializeField] private GameObject[] clonePrefabs;
    private int currentPrefabIndex = 0;
    [SerializeField] private TMP_InputField playerNameInputField;
    private void Start()
    {
        nameResourcesPrefabSelected = clonePrefabs[currentPrefabIndex].name;
    }
    public void ChangeSkin(int direct)
    {
        clonePrefabs[currentPrefabIndex].SetActive(false);
        currentPrefabIndex += direct;
        if (currentPrefabIndex < 0) currentPrefabIndex = clonePrefabs.Length - 1;
        if (currentPrefabIndex >= clonePrefabs.Length) currentPrefabIndex = 0;
        nameResourcesPrefabSelected = clonePrefabs[currentPrefabIndex].name;
        clonePrefabs[currentPrefabIndex].SetActive(true);
    }
    public void Go()
    {
        PlayerName = playerNameInputField.text;
        SceneManager.LoadScene("GameScene");
    }
}
