using UnityEngine;

public class EnemyLifeManager : LifeManager
{
    [SerializeField] private EnemyManagerState enemyManagerState;
    
    public override void ReSpawn()
    {
        base.ReSpawn();
        enemyManagerState.characterController.enabled = true;
        enemyManagerState.enabled = true;
        enemyManagerState.SwitchToRunState();
        enemyManagerState.ContinueMove();
    }
    public override void Die()
    {
        base.Die();
        enemyManagerState.animator.Play("Death");
        enemyManagerState.enabled = false;
        enemyManagerState.characterController.enabled = false;
        enemyManagerState.StopMove();
    }
    public override void ActiveFalse()
    {
        base.ActiveFalse();
        EnemyManager.instance.ReturnEnemy(this.gameObject);
    }
}
