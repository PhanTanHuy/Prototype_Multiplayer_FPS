using UnityEngine;
using System;
using Photon.Pun;

public class HealthManager : MonoBehaviourPun
{
    [SerializeField] protected int maxHealth = 1000;

    protected int currentHealth;

    public event Action OnZeroHealth;
    public event Action OnReSpawn;
    public float PercentHealth => (float)currentHealth / maxHealth;
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public void RecoverHealthSync()
    {
        currentHealth = maxHealth;
    }
    public void SendTakeDamage(int damage)
    {
        photonView.RPC(nameof(RPC_TakeDamageSync), RpcTarget.All, damage);
    }
    [PunRPC]
    protected virtual void RPC_TakeDamageSync(int d)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= d;
        currentHealth = Mathf.Max(currentHealth, 0);
        if (currentHealth <= 0)
        {
            OnZeroHealth?.Invoke();
        }
    }
    public virtual void ReSpawn()
    {

    }
    public virtual void Die()
    {
    }
}