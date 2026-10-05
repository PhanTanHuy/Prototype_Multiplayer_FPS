using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuInGame : MonoBehaviourPunCallbacks
{
    [Header("Input")]
    [SerializeField] private InputActionReference escapeAction;

    [Header("UI")]
    [SerializeField] private GameObject menuPanel;

    private void Start()
    {
        menuPanel.SetActive(false);
        escapeAction.action.Enable();
        escapeAction.action.performed += OnEscape;
    }
    private void OnEscape(InputAction.CallbackContext context)
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        escapeAction.action.performed -= OnEscape;
        escapeAction.action.Disable();
        SceneManager.LoadScene("Menu");
    }
}