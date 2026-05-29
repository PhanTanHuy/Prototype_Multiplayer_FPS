using UnityEngine;
using System;
public class HealthManager : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    public event Action OnZeroHealth;
    private void Awake()
    {
        currentHealth = maxHealth;
    }
    public void RecoverHealth()
    {
        currentHealth = maxHealth;
    }
    public void TakeDame(int d)
    {
        currentHealth -= d;
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnZeroHealth?.Invoke();
        }
    }
}
