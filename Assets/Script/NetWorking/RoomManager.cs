using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [Header("Player Join Notification")]
    [SerializeField] private TMP_Text playerJoinText;
    [SerializeField] private float messageDuration = 5f;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoint;

    private bool hasSpawned;
    private Coroutine hideMessageCoroutine;

    private void Awake()
    {
        Instance = this;

        if (playerJoinText != null)
            playerJoinText.gameObject.SetActive(false);
    }

    private void Start()
    {
        TrySpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("RoomManager: OnJoinedRoom");

        TrySpawnPlayer();
    }

    // Được gọi trên các player ĐÃ ở trong room

    [PunRPC]
    private void RPC_ShowPlayerJoined(string playerName)
    {
        ShowPlayerJoinedMessage(playerName);
    }

    private void ShowPlayerJoinedMessage(string playerName)
    {
        if (playerJoinText == null)
            return;

        playerJoinText.text = $"{playerName} joined the game!";
        playerJoinText.gameObject.SetActive(true);

        if (hideMessageCoroutine != null)
            StopCoroutine(hideMessageCoroutine);

        hideMessageCoroutine = StartCoroutine(HideJoinMessage());
    }

    private IEnumerator HideJoinMessage()
    {
        yield return new WaitForSeconds(messageDuration);

        playerJoinText.gameObject.SetActive(false);
    }

    // =========================
    // SPAWN
    // =========================

    private void TrySpawnPlayer()
    {
        if (hasSpawned)
            return;

        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Chưa vào Room, chờ OnJoinedRoom...");
            return;
        }

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        Hashtable properties =
            PhotonNetwork.LocalPlayer.CustomProperties;

        string character = "";

        if (properties.TryGetValue("Character", out object characterValue))
        {
            character = characterValue.ToString();
        }

        if (string.IsNullOrEmpty(character))
        {
            Debug.LogError("Character không tồn tại!");
            hasSpawned = false;
            return;
        }

        Vector3 spawnPosition = GetSpawnPoint();

        GameObject player = PhotonNetwork.Instantiate(
            character,
            spawnPosition,
            Quaternion.identity
        );

        player.GetComponent<PlayerNetworkSetup>().SetLocalPlayer();

        Debug.Log(
            $"Player spawned | Character: {character} | Position: {spawnPosition}"
        );
        photonView.RPC(
            nameof(RPC_ShowPlayerJoined),
            RpcTarget.All,
            PhotonNetwork.LocalPlayer.NickName
        );
    }

    public Vector3 GetSpawnPoint()
    {
        return spawnPoint[
            Random.Range(0, spawnPoint.Length)
        ].position;
    }
}