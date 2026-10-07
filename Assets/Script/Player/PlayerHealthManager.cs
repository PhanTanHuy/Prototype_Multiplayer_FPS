using UnityEngine;

public class PlayerHealthManager : HealthManager
{
    protected override void Start()
    {
        base.Start();
        PlayerUI.instance.UpdateHealthUI(PercentHealth);
    }
    protected override void RPC_TakeDamageSync(int d)
    {
        base.RPC_TakeDamageSync(d);
        if (photonView.IsMine) PlayerUI.instance.UpdateHealthUI(PercentHealth);
    }
}
