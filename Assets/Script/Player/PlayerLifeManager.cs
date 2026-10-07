using UnityEngine;

public class PlayerLifeManager : LifeManager
{
    [SerializeField] private PlayerManagerState playerManagerState;
    protected override void Start()
    {
        base.Start();
        SetWeaponLayerToDefault();
    }

    private void SetWeaponLayerToDefault()
    {
        Transform weaponRoot = playerManagerState.weaponHolder.transform;

        foreach (Transform child in weaponRoot.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
    public override void ReSpawn() // event callback
    {
        healthManager.RecoverHealthSync();
        transform.position = RoomManager.Instance.GetSpawnPoint(); // sync
        if (!photonView.IsMine) return;
        PlayerUI.instance.TurnOffWattingImage();
        PlayerUI.instance.UpdateHealthUI(healthManager.PercentHealth);
        playerManagerState.PlayerLive();
    }
    public override void Die() // event callback, targetall
    {
        if (!photonView.IsMine) return;
        playerManagerState.PlayerDie();
        PlayerUI.instance.TurnOnWattingImage();
        Invoke(nameof(ReSpawn), 3f); 
    }
}
