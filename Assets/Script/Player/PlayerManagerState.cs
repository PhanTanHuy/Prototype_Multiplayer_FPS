using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Coherence.Toolkit; // Thêm namespace Coherence

public class PlayerManagerState : BaseManagerState<PlayerManagerState>
{
    // Tham chiếu tới Coherence
    public CoherenceSync coherenceSync { get; private set; }

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
    public PlayerNetworkSetting playerNetworkSetting;


    // state
    private PlayerStateIdle idleState = new PlayerStateIdle();
    private PlayerStateWalk walkState = new PlayerStateWalk();
    private PlayerStateRun runState = new PlayerStateRun();
    private PlayerStateJump jumpState = new PlayerStateJump();
    public bool IsHoldSprint { get; set; }

    private Coroutine cameraRotReloadCoroutine, c, changeWp;

    private int lastAnimState = -1;
    private int lastWeaponIndex = -1;

    private void Awake()
    {
        coherenceSync = GetComponentInParent<CoherenceSync>();
    }
    private void Start()
    {
        if (coherenceSync == null) Debug.Log("ko thay coherence");
        // KIỂM TRA QUYỀN: Nếu là máy khác spawn, KHÔNG setup Input tránh điều khiển đè nhau
        if (coherenceSync != null && !coherenceSync.HasStateAuthority)
        {
            return;
        }

        currentState = idleState;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Khởi tạo vũ khí mặc định
        playerNetworkSetting.netWeaponIndex = 0;
        weaponHolder.ChangeWeapon(0);

        inputActions = new InputSystemActions();
        inputActions.Enable();

        inputActions.Player.ChangeWeapon.performed += ctx =>
        {
            int targetWp = (int)ctx.ReadValue<Vector2>().y;
            ChangeWeapon(targetWp);
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

    protected override void Update()
    {
        if (coherenceSync == null) return;

        if (coherenceSync.HasStateAuthority)
        {
            base.Update();
            // CHỦ SỞ HỮU (OWNER): Đẩy dữ liệu Weight thực tế của Coroutine cục bộ lên các biến mạng
            playerNetworkSetting.netRigMoveWeight = rigMove.weight;
            playerNetworkSetting.netRigRunWeight = rigRun.weight;
            playerNetworkSetting.netRigAimWeight = rigAim.weight;
            playerNetworkSetting.netRigReloadWeight = rigRotCameraReload.weight;
            playerNetworkSetting.netRigChangeWpWeight = rigChangeWeapon.weight;
        }
        else
        {
            // MÁY KHÁC (REMOTE CLIENT): Liên tục đọc dữ liệu mạng về và ép trực tiếp vào các Rig
            rigMove.weight = playerNetworkSetting.netRigMoveWeight;
            rigRun.weight = playerNetworkSetting.netRigRunWeight;
            rigAim.weight = playerNetworkSetting.netRigAimWeight;
            rigRotCameraReload.weight = playerNetworkSetting.netRigReloadWeight;
            rigChangeWeapon.weight = playerNetworkSetting.netRigChangeWpWeight;
            // Đồng bộ Animation khi có sự thay đổi trạng thái từ Owner gửi về
            if (playerNetworkSetting.netAnimState != lastAnimState)
            {
                TriggerRemoteAnimation(playerNetworkSetting.netAnimState);
                lastAnimState = playerNetworkSetting.netAnimState;
            }

            // Đồng bộ hiển thị đổi súng trên màn hình người khác
            if (playerNetworkSetting.netWeaponIndex != lastWeaponIndex)
            {
                changeWp = StartCoroutine(IEChangeWeapon(playerNetworkSetting.netWeaponIndex));
                lastWeaponIndex = playerNetworkSetting.netWeaponIndex;
            }
        }
    }
    // Thêm hàm này vào bất kỳ đâu trong PlayerManagerState.cs

    public void SendNetworkReload()
    {
        // TẤM KHIÊN: Máy Owner gọi hàm này để mượn đường truyền mạng phát lệnh đi,
        // nhưng bản thân nó không chạy code bên dưới (vì Owner đã tự chạy ở script Gun rồi).
        if (coherenceSync != null && coherenceSync.HasStateAuthority) return;

        // Máy Client nhận lệnh sẽ đi qua tấm khiên và chạy code này:
        if (weaponHolder.currentWeapon != null)
        {
            weaponHolder.currentWeapon.PlayReloadVisuals();
            Debug.Log(gameObject.name + " là client reload ");
        }
    }
    // Thêm hàm này vào PlayerManagerState.cs cùng chỗ với hàm SendNetworkReload

    public void SendNetworkShoot(float aimValue)
    {
        // TẤM KHIÊN: Nếu là chủ sở hữu (Owner) thì KHÔNG chạy đoạn code dưới đây, 
        // vì súng của bạn đã tự chạy PlayShootVisuals() ở trong script Gun rồi.
        if (coherenceSync != null && coherenceSync.HasStateAuthority) return;

        // CÁC MÁY CLIENT KHÁC NHẬN LỆNH sẽ đi qua tấm khiên và chạy đoạn code này:
        if (weaponHolder.currentWeapon != null)
        {
            weaponHolder.currentWeapon.PlayAttackVisuals(aimValue);
            Debug.Log(gameObject.name + " là client bắn súng ");
        }
    }
    private void TriggerRemoteAnimation(int stateIndex)
    {
        // Hàm này chỉ chạy trên các máy khách để nhại lại Animation của bạn
        switch (stateIndex)
        {
            case 0: // Idle
                animator.CrossFade("Idle", 0.25f, 1);
                animator.CrossFade("Idle", 0.25f, 0);
                break;
            case 1: // Walk
                animator.CrossFade("Walk", 0.25f, 0); // Chỉnh lại theo tên chuẩn trong HandelWalkAnimation() của bạn
                break;
            case 2: // Run
                animator.CrossFade("Run", 0.15f, 1);
                animator.CrossFade("Run", 0.15f, 0);
                break;
            case 4: // Death
                animator.Play("Death");
                break;
        }
    }

    void ChangeWeapon(int vl)
    {
        if (changeWp != null) return;
        playerNetworkSetting.netWeaponIndex = vl; // Cập nhật để mạng gửi đi
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
        if (coherenceSync != null && coherenceSync.HasStateAuthority)
        {
            playerNetworkSetting.netAnimState = 4; // Trạng thái chết mạng
        }
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
        // Chỉ chạy logic tấn công nếu là chủ sở hữu nhân vật này
        if (coherenceSync != null && !coherenceSync.HasStateAuthority) return;
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
    public override void StopMove() { }
    public override void ContinueMove() { }

    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
    }
}
public class PlayerStateIdle : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;

        bms.playerNetworkSetting.netAnimState = 0; // Cập nhật mã trạng thái mạng thành 0 (Idle)
        bms.animator.CrossFade("Idle", 0.25f, 1);
        bms.animator.CrossFade("Idle", 0.25f, 0);
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        // TẤM KHIÊN CHẶN: Máy khác không được tự chuyển đổi state của tôi
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;

        if (bms.MoveInput != Vector2.zero) bms.SwitchToWalkState();
        if (bms.IsHoldSprint) bms.SwitchToRunState();
    }
    public override void ExitState(PlayerManagerState bms) { }
}

public class PlayerStateWalk : BaseState<PlayerManagerState>
{
    public override void EnterState(PlayerManagerState bms)
    {
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;

        bms.playerNetworkSetting.netAnimState = 1; // Mã trạng thái mạng thành 1 (Walk)
        bms.HandelWalkAnimation();
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;

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
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;

        bms.playerNetworkSetting.netAnimState = 2; // Mã trạng thái mạng thành 2 (Run)
        bms.animator.CrossFade("Run", 0.15f, 1);
        bms.animator.CrossFade("Run", 0.15f, 0);
    }
    public override void UpdateState(PlayerManagerState bms)
    {
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;

        if (!(bms.MoveInput.y > 0f)) bms.SwitchToIdleState();
        bms.characterController.Move(bms.transform.forward * Time.deltaTime * bms.SpeedRun);
    }
    public override void ExitState(PlayerManagerState bms)
    {
        if (bms.coherenceSync != null && !bms.coherenceSync.HasStateAuthority) return;
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


