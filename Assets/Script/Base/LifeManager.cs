using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public abstract class LifeManager : MonoBehaviourPun
{
    [SerializeField] protected HealthManager healthManager;
    [SerializeField] protected HitBox[] hitBoxes;
    protected virtual void Start()
    {
        healthManager.OnZeroHealth += Die;
        healthManager.OnReSpawn += ReSpawn;
    }
    public virtual void FirstSpawn()
    {

    }
    // can overide lai agent, switch state
    public virtual void ReSpawn()
    {
    
        foreach (HitBox hb in hitBoxes) hb.gameObject.SetActive(true);
        healthManager.RecoverHealth();
    }
    public virtual void Die()
    {
        foreach (HitBox hb in hitBoxes) hb.gameObject.SetActive(false); // sync
    }
   
}

