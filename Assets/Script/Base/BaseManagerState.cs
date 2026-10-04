using UnityEngine;
using Photon.Pun;
public abstract class BaseManagerState<T> : MonoBehaviourPun where T : BaseManagerState<T>
{
    public Animator animator;
    public CharacterController characterController;
    protected BaseState<T> currentState;
    private float walkValueBlend;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.1f;
    public float VerticalVelocity { get; protected set; }

    public Vector2 MoveInput { get; protected set; }

    protected virtual void Update()
    {
        currentState.UpdateState((T)this);
        HandleGravity();
        AllStateLogic();
    }

    protected virtual void HandleGravity()
    {
        if (CheckGrounded() && VerticalVelocity < 0f)
            VerticalVelocity = -2f;

        VerticalVelocity -= 9.8f * Time.deltaTime;
        characterController.Move(Vector3.up * VerticalVelocity * Time.deltaTime);
    }
    protected bool IsFalling()
    {
        return VerticalVelocity < -0.1f && !CheckGrounded();
    }
    public bool CheckGrounded()
    {
        Vector3 center = characterController.transform.TransformPoint(characterController.center);

        float bottom = characterController.height * 0.5f;

        Vector3 checkPosition = center + Vector3.down * (bottom + 0.05f);

        Vector3 halfExtents = new Vector3(
            characterController.radius * 0.8f,
            0.05f,
            characterController.radius * 0.8f
        );

        return Physics.CheckBox(
            checkPosition,
            halfExtents,
            Quaternion.identity,
            groundLayer
        );
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
