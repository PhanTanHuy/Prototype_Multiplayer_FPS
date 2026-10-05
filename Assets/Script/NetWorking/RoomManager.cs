using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Spawn Points")]
    public Transform[] spawnPoint;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnPlayer();
    }

    public Vector3 GetSpawnPoint()
    {
        return spawnPoint[
            Random.Range(0, spawnPoint.Length)
        ].position;
    }

    private void SpawnPlayer()
    {
        // Kiểm tra đã vào Room chưa
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Player chưa Join Room!");
            return;
        }

        // Lấy thông tin Player từ MenuManager đã lưu
        Hashtable properties =
            PhotonNetwork.LocalPlayer.CustomProperties;

        string character = "";

        if (properties.TryGetValue("Character", out object characterValue))
        {
            character = characterValue.ToString();
        }

        Debug.Log(
            $"Spawn Player |Character: {character}"
        );

        // Lấy vị trí spawn
        Vector3 spawnPosition = GetSpawnPoint();

        // Spawn Player
        GameObject player = PhotonNetwork.Instantiate(
            character,
            spawnPosition,
            Quaternion.identity
        );
        player.GetComponent<PlayerNetworkSetup>().SetLocalPlayer();
        Debug.Log(
            $"Player spawned at: {spawnPosition}"
        );
    }
}
