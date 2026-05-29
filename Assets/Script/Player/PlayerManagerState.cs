using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerManagerState : BaseManagerState<PlayerManagerState>
{
    private InputSystemActions inputActions;
    [Header("Physic Setting")]
    [SerializeField] private float speedWalk = 2f;
    [SerializeField] private float speedRun = 5f;
    public float SpeedWalk { get { return speedWalk; } }
    public float SpeedRun { get { return speedRun; } }
    [Header("Look Settings")]
   
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
    // state
    private PlayerStateIdle idleState = new PlayerStateIdle();
    private PlayerStateWalk walkState = new PlayerStateWalk();
    private PlayerStateRun runState = new PlayerStateRun();
    private PlayerStateJump jumpState = new PlayerStateJump();
    public bool IsHoldSprint { get; set; }
    //
    private Coroutine cameraRotReloadCoroutine, c, changeWp;
    //
    private void Start()
    {
        //SetRigMove();
        currentState = idleState;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        weaponHolder.ChangeWeapon(0);

        inputActions = new InputSystemActions();
        inputActions.Enable();

        inputActions.Player.ChangeWeapon.performed += ctx =>
        {
            ChangeWeapon((int)ctx.ReadValue<Vector2>().y);
        };

        inputActions.Player.Move.performed += ctx =>
        {
            MoveInput = ctx.ReadValue<Vector2>();
            if (!IsHoldSprint) HandelWalkAnimation();
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
            if (weaponHolder.currentWeapon != null) weaponHolder.currentWeapon.WeaponReload();
        };
    }
    void ChangeWeapon(int vl)
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
        weaponHolder.ChangeWeapon(vl);

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
        weaponHolder.ReleaseAllWeapon();
        rigMove.weight = 0f;
        rigRun.weight = 0f;
        rigAim.weight = 0f;
        rigRotCameraReload.weight = 0f;
        rigChangeWeapon.weight = 0f;
        animator.Play("Death");
    }
    private void ThrowWeapon()
    {
        weaponHolder.currentWeapon = null;
    }
    public void HandleAttack()
    {
        if (inputActions.Player.Attack.IsPressed()) if(weaponHolder.currentWeapon != null) weaponHolder.currentWeapon.WeaponAttack(rigAim.weight);
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
            // phai lam cho giam do nhay theo scope
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
    public override void StopMove()
    {

    }
    public override void ContinueMove()
    {
        
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
        if (bms.MoveInput != Vector2.zero)
        {
            bms.SwitchToWalkState();
        }
        if (bms.IsHoldSprint)
        {
            bms.SwitchToRunState();
        }
    }
    public override void ExitState(PlayerManagerState bms)
    {
    }
}
public class PlayerStateWalk : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
        bms.HandelWalkAnimation();
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        if (bms.MoveInput == Vector2.zero)
        {
            bms.SwitchToIdleState();
        }
        if (bms.IsHoldSprint)
        {
            bms.SwitchToRunState();
        }
        Vector3 move = bms.transform.right * bms.MoveInput.x + bms.transform.forward * bms.MoveInput.y;

        bms.characterController.Move(move * Time.deltaTime * bms.SpeedWalk);
    }
    public override void ExitState(PlayerManagerState bms)
    {
    }
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
        if (!(bms.MoveInput.y > 0f))
        {
            bms.SwitchToIdleState();
        }
        bms.characterController.Move(bms.transform.forward * Time.deltaTime * bms.SpeedRun);
    }
    public override void ExitState(PlayerManagerState bms)
    {
        bms.SetRigMove();
    }
}
public class PlayerStateJump : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
    }
    public override void UpdateState(PlayerManagerState bms)
    {

    }
    public override void ExitState(PlayerManagerState bms)
    {
    }
}


