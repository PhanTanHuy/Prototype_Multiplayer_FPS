using UnityEngine;
using UnityEngine.AI;

public abstract class LifeManager : MonoBehaviour
{
    [SerializeField] protected HealthManager healthManager;
    [SerializeField] protected HitBox[] hitBoxes;
    protected virtual void Start()
    {
        healthManager.OnZeroHealth += Die;
    }
    public virtual void FirstSpawn()
    {

    }
    // can overide lai agent, switch state
    public virtual void ReSpawn()
    {
    
        foreach (HitBox hb in hitBoxes) hb.gameObject.SetActive(true);
        gameObject.SetActive(true);
        healthManager.RecoverHealth();
    }
    public virtual void Die()
    {
        foreach (HitBox hb in hitBoxes) hb.gameObject.SetActive(false);
        Invoke("ActiveFalse", 5f);
    }
    public virtual void ActiveFalse()
    {
        gameObject.SetActive(false);
    }
}

