using UnityEngine;
using System;
public class HealthManager : MonoBehaviour
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
        }
    }
}
