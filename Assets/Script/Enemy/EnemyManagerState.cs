using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyManagerState : BaseManagerState<EnemyManagerState>
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    private EnemyIdleState idleState = new EnemyIdleState();
    private EnemyWalkState walkState = new EnemyWalkState();
    private EnemyRunState runState = new EnemyRunState();
    private EnemyAttackState attackState = new EnemyAttackState();
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float speedAttack = 0.5f;
    private float timerAttack = 0f;
    private Transform target;
    //[SerializeField] private HealthManager healthManager;
    [SerializeField] private HitBox[] hitBox;
    private void OnEnable()
    {
        target = EnemyManager.instance.GetTarget(this.transform);
        SwitchState(runState);
        //healthManager.OnZeroHealth += () =>
        //{
        //    Debug.Log("Dearth Enemy");
        //    ReturnPool();
        //};
    }
    protected override void Update()
    {
        base.Update();
    }
    public override void SwitchToIdleState()
    {
        SwitchState(idleState);
    }

    public override void SwitchToJumpState()
    {
        //SwitchState(idleState);
    }
    public void SwitchToAttackState()
    {
        SwitchState(attackState);
    }
    public override void SwitchToRunState()
    {
        SwitchState(runState);
    }

    public override void SwitchToWalkState()
    {
        SwitchState(walkState);
    }
    //state
    // idle --> walk --> run
    // walk --> idle
    // run -- > walk
    // allstate --> attack
    public class EnemyIdleState : BaseState<EnemyManagerState>
    {
        public override void EnterState(EnemyManagerState bms)
        {
            bms.animator.CrossFade("Idle", 0.25f);
        }
        public override void UpdateState(EnemyManagerState bms)
        {
        }

        public override void ExitState(EnemyManagerState bms)
        {
        }
        
    }
    public class EnemyWalkState : BaseState<EnemyManagerState>
    {
        public override void EnterState(EnemyManagerState bms)
        {
            bms.animator.CrossFade("Walk", 0.25f);
        }
        public override void UpdateState(EnemyManagerState bms)
        {
        }

        public override void ExitState(EnemyManagerState bms)
        {
        }

    }
    public class EnemyRunState : BaseState<EnemyManagerState>
    {
        public override void EnterState(EnemyManagerState bms)
        {
            bms.animator.CrossFade("Run", 0.25f);

        }
        public override void UpdateState(EnemyManagerState bms)
        {
            bms.navMeshAgent.SetDestination(bms.target.position);
            if (bms.navMeshAgent.remainingDistance < 2f && bms.timerAttack < 0f)
            {
                bms.SwitchToAttackState();
            }
            bms.timerAttack -= Time.deltaTime;
        }

        public override void ExitState(EnemyManagerState bms)
        {
        }

    }
    public class EnemyAttackState : BaseState<EnemyManagerState>
    {
        public override void EnterState(EnemyManagerState bms)
        {
            bms.animator.CrossFade("Attack", 0.25f);
        }
        public override void UpdateState(EnemyManagerState bms)
        {
        }

        public override void ExitState(EnemyManagerState bms)
        {
            bms.timerAttack = bms.speedAttack;
        }

    }
    public override void StopMove()
    {
        navMeshAgent.isStopped = true;
    }
    public override void ContinueMove()
    {
        navMeshAgent.isStopped = false;
    }
    //public void StartEnemy()
    //{
    //    navMeshAgent.isStopped = false;
    //    characterController.enabled = true;
    //    this.enabled = true;
    //    SwitchState(runState);
    //    foreach (HitBox hb in hitBox) hb.gameObject.SetActive(true);
    //    healthManager.RecoverHealth();
    //}
    //public void ReturnPool()
    //{
    //    navMeshAgent.isStopped = true;
    //    animator.Play("Death");
    //    this.enabled = false;
    //    foreach (HitBox hb in hitBox) hb.gameObject.SetActive(false);
    //    characterController.enabled = false;
    //    Invoke("ActiveFalse", 5f);
    //}
    //void ActiveFalse()
    //{
    //    gameObject.SetActive(false);
    //    EnemyManager.instance.ReturnEnemy(gameObject);
    //}
}
