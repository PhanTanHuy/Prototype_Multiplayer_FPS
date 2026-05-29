using UnityEngine;

public abstract class BaseManagerState<T> : MonoBehaviour where T : BaseManagerState<T>
{
    public Animator animator;
    public CharacterController characterController;
    protected BaseState<T> currentState;
    private float walkValueBlend;
    public Vector2 MoveInput { get; protected set; }

    protected virtual void Update()
    {
        currentState.UpdateState((T)this);
        HandleGravity();
        AllStateLogic();
    }
    private void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            characterController.Move(9.8f * Vector3.down * Time.deltaTime);
        }
    }
    public void SwitchState(BaseState<T> newState)
    {
        currentState?.ExitState((T)this); 
        currentState = newState;
        currentState.EnterState((T)this);
    }
    public void HandelWalkAnimation()
    {
        if (MoveInput == Vector2.zero)
        {
            return;
        }

        Vector2 dir = MoveInput.normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            if (dir.x < 0)
            {
                animator.CrossFade("WalkLeft", 0.25f);
            }
              
            else
            {
                animator.CrossFade("WalkRight", 0.25f);
            }
        }
        else
        {
            if (dir.y < 0)
            {
                animator.CrossFade("WalkBack", 0.25f);
            }
            else
            {
                animator.CrossFade("Walk", 0.25f);
            }
        }
    }
    public virtual void AllStateLogic()
    {
       
    }

    public abstract void SwitchToWalkState();
    public abstract void SwitchToIdleState();
    public abstract void SwitchToRunState();
    public abstract void SwitchToJumpState();
    public abstract void StopMove();
    public abstract void ContinueMove();
    
}
