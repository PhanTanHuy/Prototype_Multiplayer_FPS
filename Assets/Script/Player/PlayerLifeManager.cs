using UnityEngine;

public class PlayerLifeManager : LifeManager
{
    [SerializeField] private PlayerManagerState playerManagerState;
   
    public override void ReSpawn()
    {
        base.ReSpawn();
        playerManagerState.characterController.enabled = true;
        playerManagerState.enabled = true;
        playerManagerState.ContinueMove();
    }
    public override void Die()
    {
        base.Die();
        //playerManagerState.PlayerDie();
        //playerManagerState.animator.Play("Death");
        //playerManagerState.enabled = false;
        //playerManagerState.characterController.enabled = false;
        //playerManagerState.StopMove();
        Debug.Log("PLAYER DIEEEEEEEEEEEEEEEE");
    }
    public override void ActiveFalse()
    {
        
    }
}
