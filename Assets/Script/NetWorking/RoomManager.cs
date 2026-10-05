using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Room Settings")]
    [SerializeField] private string roomName = "TestRoom";
    [SerializeField] private byte maxPlayers = 4;
    [SerializeField] private GameObject loadingUI;

    public Transform[] spawnPoint;

    private void Awake()
    {
        Instance = this;
    }

   
    public Vector3 GetSpawnPoint()
    {
        return spawnPoint[Random.Range(0, spawnPoint.Length)].position;
    }
    
}