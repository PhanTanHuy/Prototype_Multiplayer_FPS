using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public static MenuManager Instance;

    [Header("Create Room UI")]
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_Text roomErrorText;

    [Header("Selected Settings UI")]
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text mapText;
    [SerializeField] private TMP_Text playTimeText;

    [Header("Room List")]
    [SerializeField] private Transform roomListContent;
    [SerializeField] private RoomItem roomItemPrefab;

    // =========================
    // ROOM SETTINGS
    // =========================

    private readonly string[] modes =
    {
        "Solo",
        "Team"
    };

    private readonly int[] maxPlayers =
    {
        4,
        8,
        12
    };

    private readonly string[] maps =
    {
        "Backroom",
    };

    private readonly int[] playTimes =
    {
        5,
        10,
        15
    };

    private int modeIndex = 0;
    private int playerIndex = 1;
    private int mapIndex = 0;
    private int playTimeIndex = 1;

    private void Awake()
    {
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        UpdateSettingsUI();

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    // =========================================================
    // PHOTON CONNECTION
    // =========================================================

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");

        // Photon bắt đầu gửi Room List
    }

    // =========================================================
    // CREATE ROOM
    // =========================================================

    public void CreateAndJoinUI()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Photon chưa kết nối.");
            return;
        }

        string roomName = roomNameInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Room name không được để trống.");
            return;
        }

        CreateRoom(roomName);
    }

    private void CreateRoom(string roomName)
    {
        // Xóa thông báo cũ
        roomErrorText.text = "";

        roomName = roomName.Trim();

        // ==============================
        // CHECK ROOM NAME
        // ==============================

        if (string.IsNullOrEmpty(roomName))
        {
            roomErrorText.text = "Tên phòng không được để trống!";
            return;
        }

        if (roomName.Length > 13)
        {
            roomErrorText.text = "Tên phòng không được dài quá 13 ký tự!";
            return;
        }


        // ==============================
        // PLAYER INFORMATION
        // ==============================

        string playerName =
            MenuCustomPlayer.Instance.GetPlayerName();

        string character =
            MenuCustomPlayer.Instance.GetCharacter();

        PhotonNetwork.NickName = playerName;

        Hashtable playerProperties = new Hashtable
    {
        { "PlayerName", playerName },
        { "Character", character }
    };

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            playerProperties
        );


        // ==============================
        // ROOM INFORMATION
        // ==============================

        Hashtable roomProperties = new Hashtable
    {
        { "Mode", modes[modeIndex] },
        { "Map", maps[mapIndex] },
        { "PlayTime", playTimes[playTimeIndex] }
    };

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers[playerIndex],

            IsVisible = true,
            IsOpen = true,

            CustomRoomProperties = roomProperties,

            CustomRoomPropertiesForLobby = new string[]
            {
            "Mode",
            "Map",
            "PlayTime"
            }
        };

        Debug.Log(
            $"Creating Room: {roomName}"
        );

        PhotonNetwork.CreateRoom(
            roomName,
            roomOptions
        );
    }
    public override void OnCreatedRoom()
    {
        Debug.Log(
            "Room created: " +
            PhotonNetwork.CurrentRoom.Name
        );
    }

    // =========================================================
    // JOIN ROOM
    // =========================================================

    public void JoinUI(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Photon chưa kết nối.");
            return;
        }

        Debug.Log(
            "Joining Room: " +
            roomName
        );

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(
            "Joined Room: " +
            PhotonNetwork.CurrentRoom.Name
        );

        Debug.Log(
            "Players: " +
            PhotonNetwork.CurrentRoom.PlayerCount +
            "/" +
            PhotonNetwork.CurrentRoom.MaxPlayers
        );

        // Lưu thông tin player khi JOIN room
        string playerName =
            MenuCustomPlayer.Instance.GetPlayerName();

        string character =
            MenuCustomPlayer.Instance.GetCharacter();

        PhotonNetwork.NickName = playerName;

        Hashtable playerProperties = new Hashtable
        {
            { "PlayerName", playerName },
            { "Character", character }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            playerProperties
        );

        // ------------------------------------
        // MASTER LOAD GAME
        // ------------------------------------

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Map", out object map)) PhotonNetwork.LoadLevel(map.ToString());
            else Debug.LogWarning("Map not found in room properties.");
        }

        // Nếu KHÔNG phải Master:
        // Không tự LoadScene.
        //
        // AutomaticallySyncScene = true
        // sẽ đồng bộ scene theo Master.
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

    // =========================================================
    // ROOM LIST
    // =========================================================

    public override void OnRoomListUpdate(
        List<RoomInfo> roomList)
    {
        Debug.Log(
            "Room List Updated: " +
            roomList.Count
        );

        ClearRoomList();
        int roomCount = 0;
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
                continue;

            CreateRoomItem(room);
            roomCount++;
        }
        roomListContent.GetChild(0).gameObject.SetActive(roomCount == 0);
    }

    private void CreateRoomItem(RoomInfo room)
    {
        RoomItem item =
            Instantiate(
                roomItemPrefab,
                roomListContent
            );

        item.Setup(room);
    }

    private void ClearRoomList()
    {
        for (int i = roomListContent.childCount - 1; i >= 1; i--)
        {
            Destroy(roomListContent.GetChild(i).gameObject);
        }
    }

    // =========================================================
    // ROOM SETTINGS
    // =========================================================

    public void ChangeMode(int direction)
    {
        modeIndex += direction;

        if (modeIndex < 0)
            modeIndex = modes.Length - 1;

        if (modeIndex >= modes.Length)
            modeIndex = 0;

        UpdateSettingsUI();
    }

    public void ChangePlayers(int direction)
    {
        playerIndex += direction;

        if (playerIndex < 0)
            playerIndex = maxPlayers.Length - 1;

        if (playerIndex >= maxPlayers.Length)
            playerIndex = 0;

        UpdateSettingsUI();
    }

    public void ChangeMap(int direction)
    {
        mapIndex += direction;

        if (mapIndex < 0)
            mapIndex = maps.Length - 1;

        if (mapIndex >= maps.Length)
            mapIndex = 0;

        UpdateSettingsUI();
    }

    public void ChangePlayTime(int direction)
    {
        playTimeIndex += direction;

        if (playTimeIndex < 0)
            playTimeIndex = playTimes.Length - 1;

        if (playTimeIndex >= playTimes.Length)
            playTimeIndex = 0;

        UpdateSettingsUI();
    }

    private void UpdateSettingsUI()
    {
        if (modeText != null)
            modeText.text = modes[modeIndex];

        if (playersText != null)
            playersText.text =
                maxPlayers[playerIndex].ToString();

        if (mapText != null)
            mapText.text = maps[mapIndex];

        if (playTimeText != null)
            playTimeText.text =
                playTimes[playTimeIndex] + "m";
    }
    public void SearchRoom(string searchText)
    {
        searchText = searchText.Trim().ToLower();

        int visibleRoomCount = 0;

        for (int i = 1; i < roomListContent.childCount; i++)
        {
            RoomItem roomItem =
                roomListContent.GetChild(i).GetComponent<RoomItem>();

            bool match =
                string.IsNullOrEmpty(searchText) ||
                roomItem.GetRoomName().ToLower().Contains(searchText);

            roomItem.gameObject.SetActive(match);

            if (match)
                visibleRoomCount++;
        }

        roomListContent.GetChild(0)
            .gameObject
            .SetActive(visibleRoomCount == 0);
    }
}