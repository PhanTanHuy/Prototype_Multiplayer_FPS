using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    // =========================================================
    // PLAYER JOIN NOTIFICATION
    // =========================================================

    [Header("Player Join Notification")]
    [SerializeField] private TMP_Text playerJoinText;
    [SerializeField] private float messageDuration = 5f;

    private Coroutine hideMessageCoroutine;


    // =========================================================
    // SPAWN
    // =========================================================

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoint;


    // =========================================================
    // MINIMAP
    // =========================================================

    [Header("Minimap")]
    public MinimapCamera minimapCamera;


    // =========================================================
    // PLAYER
    // =========================================================

    private bool hasSpawned;


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        Instance = this;
        minimapCamera.enabled = false;
        if (playerJoinText != null)
            playerJoinText.gameObject.SetActive(false);
    }

    private void Start()
    {
        TrySpawnPlayer();
    }


    // =========================================================
    // PHOTON ROOM
    // =========================================================

    public override void OnJoinedRoom()
    {
        Debug.Log("RoomManager: OnJoinedRoom");

        TrySpawnPlayer();
    }


    // =========================================================
    // PLAYER JOIN
    // =========================================================

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(
            $"Player joined: {newPlayer.NickName}"
        );

        photonView.RPC(
            nameof(RPC_ShowPlayerJoined),
            RpcTarget.All,
            newPlayer.NickName
        );
    }

    [PunRPC]
    private void RPC_ShowPlayerJoined(string playerName)
    {
        ShowPlayerJoinedMessage(playerName);
    }

    public void ShowPlayerJoinedMessage(string playerName)
    {
        if (playerJoinText == null)
            return;

        playerJoinText.text =
            $"{playerName} joined the game!";

        playerJoinText.gameObject.SetActive(true);

        if (hideMessageCoroutine != null)
            StopCoroutine(hideMessageCoroutine);

        hideMessageCoroutine =
            StartCoroutine(HideJoinMessage());
    }

    private IEnumerator HideJoinMessage()
    {
        yield return new WaitForSeconds(
            messageDuration
        );

        if (playerJoinText != null)
            playerJoinText.gameObject.SetActive(false);
    }


    // =========================================================
    // SPAWN PLAYER
    // =========================================================

    private void TrySpawnPlayer()
    {
        if (hasSpawned)
            return;

        if (!PhotonNetwork.InRoom)
        {
            Debug.Log(
                "Chưa vào Room, chờ OnJoinedRoom..."
            );

            return;
        }

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;


        // =====================================================
        // GET CHARACTER
        // =====================================================

        Hashtable properties =
            PhotonNetwork.LocalPlayer.CustomProperties;

        string character = "";

        if (properties.TryGetValue(
            "Character",
            out object characterValue))
        {
            character =
                characterValue.ToString();
        }

        if (string.IsNullOrEmpty(character))
        {
            Debug.LogError(
                "Character không tồn tại!"
            );

            hasSpawned = false;

            return;
        }


        // =====================================================
        // GET SPAWN POSITION
        // =====================================================

        Vector3 spawnPosition =
            GetSpawnPoint();


        // =====================================================
        // SPAWN
        // =====================================================

        GameObject player =
            PhotonNetwork.Instantiate(
                character,
                spawnPosition,
                Quaternion.identity
            );


        // =====================================================
        // SET LOCAL PLAYER
        // =====================================================

        PlayerNetworkSetup networkSetup =
            player.GetComponent<PlayerNetworkSetup>();

        if (networkSetup != null)
        {
            networkSetup.SetLocalPlayer();
        }
        minimapCamera.enabled = true;
        RoomManager.Instance.minimapCamera.follow = this.transform;

        Debug.Log(
            $"Player spawned | " +
            $"Character: {character} | " +
            $"Position: {spawnPosition}"
        );
    }


    // =========================================================
    // SPAWN POINT
    // =========================================================

    public Vector3 GetSpawnPoint()
    {
        if (spawnPoint == null ||
            spawnPoint.Length == 0)
        {
            Debug.LogError(
                "RoomManager: Chưa có Spawn Point!"
            );

            return Vector3.zero;
        }

        return spawnPoint[
            Random.Range(
                0,
                spawnPoint.Length
            )
        ].position;
    }
}