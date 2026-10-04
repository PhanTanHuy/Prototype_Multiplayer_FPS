using UnityEngine;

public class PlayerLifeManager : LifeManager
{
    [SerializeField] private PlayerManagerState playerManagerState;
   
    public override void ReSpawn()
    {
        base.ReSpawn(); //sync
        transform.position = RoomManager.Instance.GetSpawnPoint(); // sync
        if (!photonView.IsMine) return;
        playerManagerState.PlayerLive();
    }
    public override void Die()
    {
        base.Die(); // sync
        if (!photonView.IsMine) return;
        playerManagerState.PlayerDie(); 
        Invoke(nameof(ReSpawn), 3f); 
    }
    private void SendReSpawn()
    {
        healthManager.playerNetworkSetup.SendReSpawn();
    }

}
