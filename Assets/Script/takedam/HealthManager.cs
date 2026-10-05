using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;

public class HealthManager : MonoBehaviourPun
{
    [SerializeField] private int maxHealth = 100;

    [Header("Health UI")]
    [SerializeField] private Image healthImage;

    public PlayerNetworkSetup playerNetworkSetup;

    private int currentHealth;

    public event Action OnZeroHealth;
    public event Action OnReSpawn;

    private void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void ReSpawn()
    {
        RecoverHealth();
        OnReSpawn?.Invoke();
    }

    public void RecoverHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDameSync(int d)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= d;
        currentHealth = Mathf.Max(currentHealth, 0);
        if (photonView.IsMine) UpdateHealthUI();
        if (currentHealth <= 0)
        {
            OnZeroHealth?.Invoke();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthImage == null)
            return;

        healthImage.fillAmount = (float)currentHealth / maxHealth;
    }
}