using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuInGame : MonoBehaviourPunCallbacks
{
    [Header("Input")]
    [SerializeField] private InputActionReference escapeAction;

    public static MenuInGame Instance;

    [Header("UI")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject wattingImage;

    [Header("Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        menuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Slider
        sensitivitySlider.minValue = 1f;
        sensitivitySlider.maxValue = 20f;

        // Giá trị mặc định
        sensitivitySlider.value = 5f;

        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        // Input
        escapeAction.action.Enable();
        escapeAction.action.performed += OnEscape;
    }

    private void OnEscape(InputAction.CallbackContext context)
    {
        bool isMenuActive = !menuPanel.activeSelf;
        if (isMenuActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        menuPanel.SetActive(isMenuActive);

        CameraHolder.instance.StopPlayer(!isMenuActive);
    }

    private void OnSensitivityChanged(float value)
    {
        CameraHolder.instance.SetMouseSensitivity(value);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom)
            return;

        wattingImage.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        escapeAction.action.performed -= OnEscape;
        escapeAction.action.Disable();

        SceneManager.LoadScene("Menu");
    }

    private void OnDestroy()
    {
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);

        if (escapeAction != null)
            escapeAction.action.performed -= OnEscape;
    }
}