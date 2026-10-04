using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerManagerState : BaseManagerState<PlayerManagerState>
{
    private bool isDead;
    public bool IsDead => isDead;
    private InputSystemActions inputActions;
    [Header("Physic Setting")]
    [SerializeField] private float speedWalk = 2f;
    [SerializeField] private float speedRun = 5f;
    public float SpeedWalk { get { return speedWalk; } }
    public float SpeedRun { get { return speedRun; } }

    [Header("Rig References")]
    [SerializeField] private Rig rigMove;
    [SerializeField] private Rig rigRun;
    [SerializeField] private Rig rigAim;
    [SerializeField] private Rig rigRotCameraReload;
    [SerializeField] private Rig rigChangeWeapon;
    [SerializeField] private RigBuilder rigBuilder;
    [Header("Weapon Setting")]
    [SerializeField] private WeaponHolder weaponHolder;
    [Header("Camera Setting")]
    public CameraHolder cameraHolder;
    [SerializeField] private Transform pivotCameraMove;
    [Header("Network Setting")]
    public PlayerNetworkSetup playerNetworkSetup;

    // state
    private PlayerStateIdle idleState = new PlayerStateIdle();
    private PlayerStateWalk walkState = new PlayerStateWalk();
    private PlayerStateRun runState = new PlayerStateRun();
    private PlayerStateJump jumpState = new PlayerStateJump();
    public bool IsHoldSprint { get; set; }

    private Coroutine cameraRotReloadCoroutine, c, changeWp;

    private int lastAnimState = -1;
    private int lastWeaponIndex = -1;

    private void Start()
    {
        currentState = idleState;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputActions = new InputSystemActions();
        inputActions.Enable();

        inputActions.Player.ChangeWeapon.performed += ctx =>
        {
            int targetWp = (int)ctx.ReadValue<Vector2>().y;
            ChangeWeapon(targetWp);
            playerNetworkSetup.RequestChangeWeapon(targetWp); // Gửi yêu cầu thay đổi vũ khí qua mạng
        };

        inputActions.Player.Move.performed += ctx =>
        {
            MoveInput = ctx.ReadValue<Vector2>();
            if (!IsHoldSprint && currentState != jumpState) HandelWalkAnimation();
        };
        inputActions.Player.Jump.performed += ctx =>
        {
            if (CheckGrounded())
                SwitchToJumpState();
        };
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Escape.performed += ctx =>
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        };

        inputActions.Player.Sprint.performed += ctx =>
        {
            if (currentState == jumpState) return;
            SwitchToRunState();
            IsHoldSprint = true;
        };
        inputActions.Player.Sprint.canceled += ctx =>
        {
            ExitRunState();
            IsHoldSprint = false;
        };

        inputActions.Player.Aim.performed += ctx =>
        {
            if (currentState != runState) SetRigAim();
        };

        inputActions.Player.Attack.performed += ctx =>
        {
            if (currentState == runState) SetRigMove();
        };
        inputActions.Player.Attack.canceled += ctx =>
        {
            if (currentState == runState) SetRigRun();
        };

        inputActions.Player.Reload.performed += ctx =>
        {
            if (weaponHolder.currentWeapon != null)
            {
                weaponHolder.currentWeapon.WeaponReload();
                playerNetworkSetup.SendReload(); 
            }
        };
    }

    protected override void Update()
    {
        if (isDead) return;
        base.Update();
    }
    // Thêm hàm này vào bất kỳ đâu trong PlayerManagerState.cs


    public void Jump()
    {
        VerticalVelocity = 7f;
        animator.CrossFade("Jumping", 0f);
    }
    protected override void HandleGravity()
    {
        base.HandleGravity();
        if (IsFalling() && currentState != jumpState)
        {
            SwitchToJumpState();
        }
    }
    public void ChangeWeapon(int vl)
    {
        if (changeWp != null) return;
        changeWp = StartCoroutine(IEChangeWeapon(vl));
    }

    IEnumerator IEChangeWeapon(int vl)
    {
        float startT = 0f;
        float timeChange = 0.5f;
        float halfTime = timeChange / 2f;
        while (startT < halfTime)
        {
            rigChangeWeapon.weight = Mathf.Lerp(0f, 1f, startT / halfTime);
            startT += Time.deltaTime;
            yield return null;
        }
        weaponHolder.ChangeWeaponLocal(vl);

        if (rigAim.weight == 1f) cameraHolder.SetAimSentivity(weaponHolder.currentWeapon.AimSentivity);
        else cameraHolder.ResetAimSentivity();
        while (startT < timeChange)
        {
            rigChangeWeapon.weight = Mathf.Lerp(1f, 0f, (startT - halfTime) / halfTime);
            startT += Time.deltaTime;
            yield return null;
        }
        rigChangeWeapon.weight = 0f;
        changeWp = null;
    }

    public void CameraRotReload()
    {
        if (cameraRotReloadCoroutine != null) StopCoroutine(cameraRotReloadCoroutine);
        cameraRotReloadCoroutine = StartCoroutine(IECameraRotReload());
    }

    IEnumerator IECameraRotReload()
    {
        float timeReload = 0f;
        while (timeReload < 2f)
        {
            rigRotCameraReload.weight = Mathf.Lerp(0f, 1f, timeReload / 2f);
            timeReload += Time.deltaTime;
            yield return null;
        }
        rigRotCameraReload.weight = 0f;
        Quaternion t = pivotCameraMove.localRotation;
        timeReload = 0f;
        while (timeReload < 1f)
        {
            pivotCameraMove.localRotation = Quaternion.Lerp(t, Quaternion.identity, timeReload / 1f);
            timeReload += Time.deltaTime;
            yield return null;
        }
        pivotCameraMove.localRotation = Quaternion.identity;
    }

    public void PlayerDie()
    {
        if (isDead) return;
        characterController.enabled = false;
        isDead = true;
        cameraHolder.enabled = false;
        inputActions.Disable();

        weaponHolder.Die();
    }
    public void PlayerLive()
    {
        isDead = false;
        characterController.enabled = true;
        cameraHolder.enabled = true;

        VerticalVelocity = 0f;
        MoveInput = Vector2.zero;
        IsHoldSprint = false;
        weaponHolder.Live();
        inputActions.Enable();
        SwitchToIdleState();
    }

    private void ThrowWeapon()
    {
        weaponHolder.currentWeapon = null;
    }

    public void HandleAttack()
    {
        if (inputActions != null && inputActions.Player.Attack.IsPressed())
            if (weaponHolder.currentWeapon != null) weaponHolder.currentWeapon.WeaponAttack(rigAim.weight);
    }

    public void SetRigMove()
    {
        if (c != null) StopCoroutine(c);
        c = StartCoroutine(IESetRig(1f, 0f, 0f, 0.15f));
    }

    public void SetRigRun()
    {
        if (c != null) StopCoroutine(c);
        c = StartCoroutine(IESetRig(0f, 1f, 0f, 0.5f));
    }

    public void SetRigAim()
    {
        if (currentState == runState) SwitchToIdleState();
        if (c != null) StopCoroutine(c);
        if (rigAim.weight < 0.1f)
        {
            c = StartCoroutine(IESetRig(1f, 0f, 1f, 0.1f));
            weaponHolder.weaponSway.StopSway();
            cameraHolder.SetViewAim();
            cameraHolder.SetAimSentivity(weaponHolder.currentWeapon.AimSentivity);
            PlayerUI.instance.GoToAimMode();
        }
        else
        {
            SetRigMove();
            weaponHolder.weaponSway.StartSway();
            cameraHolder.SetViewDefault();
            cameraHolder.ResetAimSentivity();
            PlayerUI.instance.GoToNoneAimMode();
        }
    }

    IEnumerator IESetRig(float fm, float fr, float fa, float timeCountDown)
    {
        float count = 1f;
        while (!(count < 0.01f))
        {
            rigMove.weight = Mathf.Lerp(rigMove.weight, fm, Time.deltaTime / timeCountDown);
            rigRun.weight = Mathf.Lerp(rigRun.weight, fr, Time.deltaTime / timeCountDown);
            rigAim.weight = Mathf.Lerp(rigAim.weight, fa, Time.deltaTime / timeCountDown);
            count = Mathf.Lerp(count, 0f, Time.deltaTime / timeCountDown);
            yield return null;
        }
        rigMove.weight = fm;
        rigRun.weight = fr;
        rigAim.weight = fa;
    }

    public override void AllStateLogic()
    {
        HandleAttack();
    }

    public override void SwitchToIdleState()
    {
        SwitchState(idleState);
    }
    public override void SwitchToWalkState()
    {
        SwitchState(walkState);
    }
    public void SendWeaponAttack(float vl)
    {
        playerNetworkSetup.SendWeaponAttack(vl);
    }
    public override void SwitchToRunState()
    {
        if (MoveInput.y > 0f)
        {
            SwitchState(runState);
            if (!inputActions.Player.Attack.IsPressed()) SetRigRun();
        }
    }
    public void ExitRunState()
    {
        if (currentState == runState)
        {
            SwitchToIdleState();
        }
    }
    public override void SwitchToJumpState()
    {
        SwitchState(jumpState);
    }
    public override void StopMove() { }
    public override void ContinueMove() { }

    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
    }
    ////////////////////////////NETWORK FUNCTIONS////////////////////////////
    public int GetNetworkAnimationState()
    {
        if (isDead) return 5;
        switch (currentState)
        {
            case PlayerStateIdle:
                return 0;

            case PlayerStateWalk:
                return 1;

            case PlayerStateRun:
                return 2;

            case PlayerStateJump:
                return VerticalVelocity > 0f ? 3 : 4;

            default:
                return -1;
        }
    }
    private void ApplyNetworkAnimatorState(int stateIndex)
    {
        switch (stateIndex)
        {
            case 0:
                animator.CrossFade("Idle", 0.25f);
                break;

            case 1:
                animator.CrossFade("Walk", 0.25f);
                break;

            case 2:
                animator.CrossFade("Run", 0.15f);
                break;

            case 3:
                animator.CrossFade("Jumping", 0.1f);
                break;

            case 4:
                animator.CrossFade("Falling", 0.1f);
                break;

            case 5:
                animator.Play("Death");
                break;
        }
    }
    public void PlayNetworkReload()
    {
        weaponHolder.currentWeapon.WeaponReload();
    }
  
    public float GetNetworkCameraPitch()
    {
        return cameraHolder.GetNetworkPitch();
    }
    public void ApplyNetworkCameraPitch(float pitch)
    {
        cameraHolder.SetNetworkPitch(pitch);
    }
    public void ApplyNetworkAnimation(int stateIndex, bool aim)
    {
        if (photonView != null && photonView.IsMine)
            return;

        ApplyNetworkAnimatorState(stateIndex);
    }
    private Coroutine networkRigCoroutine;

    public void ApplyNetworkRig(int rigMode)
    {
        if (photonView != null && photonView.IsMine)
            return;

        float move = 1f;
        float run = 0f;
        float aim = 0f;

        if (rigMode == 1)
        {
            move = 0f;
            run = 1f;
        }
        else if (rigMode == 2)
        {
            move = 1f;
            aim = 1f;
        }

        if (networkRigCoroutine != null)
            StopCoroutine(networkRigCoroutine);

        networkRigCoroutine = StartCoroutine(IENetworkRig(move, run, aim));
    }
    private IEnumerator IENetworkRig(float move, float run, float aim)
    {
        float startMove = rigMove.weight;
        float startRun = rigRun.weight;
        float startAim = rigAim.weight;

        float time = 0f;
        float duration = 0.1f;

        while (time < duration)
        {
            rigMove.weight = Mathf.Lerp(startMove, move, time / duration);
            rigRun.weight = Mathf.Lerp(startRun, run, time / duration);
            rigAim.weight = Mathf.Lerp(startAim, aim, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        rigMove.weight = move;
        rigRun.weight = run;
        rigAim.weight = aim;
    }
    public int GetNetworkRigMode()
    {
        if (rigAim.weight > 0.5f) return 2;
        if (rigRun.weight > 0.5f) return 1;
        return 0;
    }
    public bool IsAiming()
    {
        return rigAim.weight > 0.1f;
    }
}
public class PlayerStateIdle : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
        bms.animator.CrossFade("Idle", 0.25f, 1);
        bms.animator.CrossFade("Idle", 0.25f, 0);
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        if (bms.MoveInput != Vector2.zero) bms.SwitchToWalkState();
        if (bms.IsHoldSprint) bms.SwitchToRunState();
    }
    public override void ExitState(PlayerManagerState bms) { }
}

public class PlayerStateWalk : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
        bms.HandelWalkAnimation();
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        if (bms.MoveInput == Vector2.zero) bms.SwitchToIdleState();
        if (bms.IsHoldSprint) bms.SwitchToRunState();

        Vector3 move = bms.transform.right * bms.MoveInput.x + bms.transform.forward * bms.MoveInput.y;
        bms.characterController.Move(move * Time.deltaTime * bms.SpeedWalk);
    }
    public override void ExitState(PlayerManagerState bms) { }
}

public class PlayerStateRun : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
        bms.animator.CrossFade("Run", 0.15f, 1);
        bms.animator.CrossFade("Run", 0.15f, 0);
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        if (!(bms.MoveInput.y > 0f)) bms.SwitchToIdleState();
        bms.characterController.Move(bms.transform.forward * Time.deltaTime * bms.SpeedRun);
    }
    public override void ExitState(PlayerManagerState bms)
    {
        bms.SetRigMove();
    }
}
public class PlayerStateJump : BaseState<PlayerManagerState>
{
    private bool hasLeftGround;

    public override void EnterState(PlayerManagerState bms)
    {
        hasLeftGround = false;
        bms.Jump();
    }

    public override void UpdateState(PlayerManagerState bms)
    {
        if (!hasLeftGround)
        {
            if (!bms.CheckGrounded())
                hasLeftGround = true;

            return;
        }

        if (bms.VerticalVelocity <= 0f)

        {
            bms.animator.CrossFade("Falling", 0.1f);
        }
        Vector3 move = bms.transform.right * bms.MoveInput.x + bms.transform.forward * bms.MoveInput.y;
        float speed = bms.IsHoldSprint ? bms.SpeedRun : bms.SpeedWalk;
        bms.characterController.Move(move * Time.deltaTime * speed);
        if (bms.CheckGrounded() && bms.VerticalVelocity < 0f)
        {
            Debug.Log("Landed from jump");
            if (bms.MoveInput != Vector2.zero)
                bms.SwitchToWalkState();
            else
                bms.SwitchToIdleState();
        }
    }

    public override void ExitState(PlayerManagerState bms)
    {
    }
}


