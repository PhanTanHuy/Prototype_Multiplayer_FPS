using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [Header("Room Settings")]
    [SerializeField] private string roomName = "TestRoom";
    [SerializeField] private byte maxPlayers = 4;
    [SerializeField] private GameObject loadingUI;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        loadingUI.SetActive(true);
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ConnectToPhoton();
    }

    private void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Already connected to Photon.");
            return;
        }

        Debug.Log("Connecting to Photon...");

        PhotonNetwork.ConnectUsingSettings();
    }

    // =========================
    // CONNECTION
    // =========================

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");

        JoinOrCreateRoom();
    }

    // =========================
    // ROOM
    // =========================

    private void JoinOrCreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            roomOptions,
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(
            $"Joined Room: {PhotonNetwork.CurrentRoom.Name}"
        );

        Debug.Log(
            $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}"
        );

        SpawnPlayer();
        loadingUI.SetActive(false);
    }

    public override void OnJoinRoomFailed(
        short returnCode,
        string message)
    {
        Debug.LogError(
            $"Join Room Failed: {returnCode} - {message}"
        );
    }

    public override void OnCreateRoomFailed(
        short returnCode,
        string message)
    {
        Debug.LogError(
            $"Create Room Failed: {returnCode} - {message}"
        );
    }

    // =========================
    // PLAYER
    // =========================

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab is not assigned!");
            return;
        }

        Vector3 position = Vector3.zero;

        if (spawnPoint != null)
        {
            position = spawnPoint.position;
        }

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            position,
            Quaternion.identity
        );
        player.GetComponent<PlayerNetworkSetup>().SetLocalPlayer();
        Debug.Log(
            $"Spawned Player: {player.name}"
        );
    }

    // =========================
    // DISCONNECT
    // =========================

    public override void OnDisconnected(
        DisconnectCause cause)
    {
        Debug.LogError(
            $"Disconnected from Photon: {cause}"
        );
    }
}