using UnityEngine;

public class PlayerLifeManager : LifeManager
{
    [SerializeField] private PlayerManagerState playerManagerState;
   
    public override void ReSpawn()
    {
        healthManager.RecoverHealth();
        transform.position = RoomManager.Instance.GetSpawnPoint(); // sync
        if (!photonView.IsMine) return;
        PlayerUI.instance.TurnOffWattingImage();
        playerManagerState.PlayerLive();
    }
    public override void Die()
    {
        if (!photonView.IsMine) return;
        playerManagerState.PlayerDie();
        PlayerUI.instance.TurnOnWattingImage();
        Invoke(nameof(ReSpawn), 3f); 
    }
    private void SendReSpawn()
    {
        healthManager.playerNetworkSetup.SendReSpawn();
    }

}
