using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [SerializeField] private Transform[] spawnPoint;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }


    private void SpawnPlayer()
    {
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
            MenuInGame.Instance.LeaveRoom();
            return;
        }

        Vector3 spawnPosition = GetSpawnPoint();

        GameObject player = PhotonNetwork.Instantiate(
            character,
            spawnPosition,
            Quaternion.identity
        );

        player
            .GetComponent<PlayerNetworkSetup>()
            .SetLocalPlayer();

        Debug.Log(
            $"Player spawned | Character: {character} | Position: {spawnPosition}"
        );
    }

    public Vector3 GetSpawnPoint()
    {
        return spawnPoint[
            Random.Range(0, spawnPoint.Length)
        ].position;
    }
}