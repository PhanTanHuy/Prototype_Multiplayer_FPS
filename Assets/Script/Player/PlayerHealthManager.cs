using UnityEngine;

public class PlayerHealthManager : HealthManager
{
    private PlayerManagerState playerManagerState;
    protected override void Start()
    {
        base.Start();
        PlayerUI.instance.UpdateHealthUI(PercentHealth);
        OnZeroHealth += Die;
        OnReSpawn += ReSpawn;
        GetComponent<PlayerNetworkSetup>().SetUpRemotePlayer();
        playerManagerState = GetComponent<PlayerManagerState>();
    }
    protected override void RPC_TakeDamageSync(int d)
    {
        base.RPC_TakeDamageSync(d);
        if (photonView.IsMine) PlayerUI.instance.UpdateHealthUI(PercentHealth);
    }
    [SerializeField] protected HitBox[] hitBoxes;

    
    public override void ReSpawn() // event callback
    {
        RecoverHealthSync();
        transform.position = RoomManager.Instance.GetSpawnPoint();
        foreach (HitBox hb in hitBoxes) hb.gameObject.SetActive(true);

        if (!photonView.IsMine) return;
        PlayerUI.instance.TurnOffWattingImage();
        PlayerUI.instance.UpdateHealthUI(PercentHealth);
        playerManagerState.PlayerLive();
    }
    public override void Die() // event callback, targetall
    {
        foreach (HitBox hb in hitBoxes) hb.gameObject.SetActive(false);

        if (!photonView.IsMine) return;
        playerManagerState.PlayerDie();
        PlayerUI.instance.TurnOnWattingImage();
        Invoke(nameof(ReSpawn), 3f);
    }
}
