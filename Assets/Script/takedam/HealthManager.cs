using UnityEngine;
using System;
using Photon.Pun;
public class HealthManager : MonoBehaviourPun
{
    [SerializeField] private int maxHealth = 100;
    public PlayerNetworkSetup playerNetworkSetup;
    private int currentHealth;
    public event Action OnZeroHealth, OnReSpawn;
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    public void ReSpawn()
    {
        if (photonView.IsMine) PlayerUI.instance.TurnOffWattingImage();
        OnReSpawn?.Invoke();
    }
    public void RecoverHealth()
    {
        currentHealth = maxHealth;
    }
    public void TakeDameSync(int d)
    {
        if (currentHealth <= 0) return;
        currentHealth -= d;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnZeroHealth?.Invoke();
            if (photonView.IsMine) PlayerUI.instance.TurnOnWattingImage();
        }
    }
}
