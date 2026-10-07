using System.Collections;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [Header("FOV")]
    [SerializeField] private float runFovPercent = 25f;
    [SerializeField] private float aimFovOffset = 13f;

    private float defaultFieldOfView;
    private float targetFieldOfView;

    private bool isRunning;
    private bool isAiming;

    private Coroutine fovCoroutine;

    [Header("Network Setting")]
    public static CameraHolder instance;

    private Coroutine recoil;

    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private float maxLookUpAngle = 80f;
    [SerializeField] private Transform rootPlayer;

    public CameraShake cameraShake;
    public Camera cameramain;

    private float xRotation = 0f;
    private Vector2 mouseDelta;

    private InputSystemActions inputActions;

    [SerializeField] float mouseSmoothTime = 0.05f;

    private Vector2 currentMouseDelta;
    private Vector2 mouseDeltaVelocity;

    private float aimStateSensitivity = 1f;

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystemActions();
            instance = this;
        }

        inputActions.Enable();
    }

    private void Start()
    {
        inputActions.Player.Look.performed += ctx =>
            mouseDelta = ctx.ReadValue<Vector2>();

        inputActions.Player.Look.canceled += ctx =>
            mouseDelta = Vector2.zero;

        defaultFieldOfView = cameramain.fieldOfView;
        targetFieldOfView = defaultFieldOfView;
    }

    private void Update()
    {
        HandleCameraRotation();
    }

    // ============================================================
    // FOV
    // ============================================================

    public void SetRunFOV(bool running)
    {
        isRunning = running;

        RefreshFOV();
    }

    public void SetViewAim()
    {
        isAiming = true;

        RefreshFOV();
    }

    public void SetViewDefault()
    {
        isAiming = false;

        RefreshFOV();
    }

    private void RefreshFOV()
    {
        float target = defaultFieldOfView;

        // Chạy: +20%
        if (isRunning)
        {
            target *= 1f + runFovPercent / 100f;
        }

        // Aim: -10
        if (isAiming)
        {
            target -= aimFovOffset;
        }

        targetFieldOfView = target;

        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
        }

        fovCoroutine = StartCoroutine(IEFOV(targetFieldOfView));
    }

    private IEnumerator IEFOV(float target)
    {
        float startFOV = cameramain.fieldOfView;
        float time = 0f;

        // Thời gian phụ thuộc vào khoảng cách FOV
        float duration = 0.45f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = time / duration;

            // Smooth Lerp
            t = Mathf.SmoothStep(0f, 1f, t);

            cameramain.fieldOfView =
                Mathf.Lerp(startFOV, target, t);

            yield return null;
        }

        cameramain.fieldOfView = target;

        fovCoroutine = null;
    }

    // ============================================================
    // CAMERA ROTATION
    // ============================================================

    public void SetAimSentivity(float vl)
    {
        aimStateSensitivity = vl;
    }

    public void ResetAimSentivity()
    {
        aimStateSensitivity = 1f;
    }

    public void StopPlayer(bool stop)
    {
        if (!stop)
            inputActions.Disable();
        else
            inputActions.Enable();

        rootPlayer
            .GetComponent<PlayerManagerState>()
            .StopPlayer(stop);
    }

    private void HandleCameraRotation()
    {
        Vector2 rawMouseDelta =
            inputActions.Player.Look.ReadValue<Vector2>();

        rawMouseDelta /= aimStateSensitivity;

        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            rawMouseDelta,
            ref mouseDeltaVelocity,
            mouseSmoothTime
        );

        float mouseX =
            currentMouseDelta.x *
            mouseSensitivity *
            Time.deltaTime;

        float mouseY =
            currentMouseDelta.y *
            mouseSensitivity *
            Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(
            xRotation,
            -maxLookUpAngle,
            maxLookUpAngle
        );

        transform.localRotation =
            Quaternion.Euler(xRotation, 0f, 0f);

        rootPlayer.transform.Rotate(
            Vector3.up * mouseX
        );
    }

    // ============================================================
    // RECOIL
    // ============================================================

    public void RecoilCamera(float recoilAmmount)
    {
        if (recoil != null)
            StopCoroutine(recoil);

        recoil = StartCoroutine(
            IERecoilCamera(recoilAmmount)
        );
    }

    private IEnumerator IERecoilCamera(float recoilAmmount)
    {
        yield return null;

        if (cameraShake != null)
        {
            cameraShake.Shake(
                0.15f,
                -recoilAmmount / 2f
            );
        }

        recoilAmmount /= aimStateSensitivity;

        float timeRecoil = 0f;

        float rotY =
            recoilAmmount *
            Random.Range(-0.2f, 0.2f) /
            0.15f;

        recoilAmmount *=
            Random.Range(0.5f, 1f);

        while (timeRecoil < 0.15f)
        {
            xRotation +=
                recoilAmmount *
                Time.deltaTime /
                0.15f;

            rootPlayer.transform.Rotate(
                Vector3.up *
                rotY *
                Time.deltaTime
            );

            timeRecoil += Time.deltaTime;

            yield return null;
        }

        recoil = null;
    }

    // ============================================================
    // NETWORK
    // ============================================================

    public float GetNetworkPitch()
    {
        return xRotation;
    }

    public void SetNetworkPitch(float pitch)
    {
        xRotation = pitch;

        transform.localRotation =
            Quaternion.Euler(
                xRotation,
                0f,
                0f
            );
    }

    // ============================================================
    // SENSITIVITY
    // ============================================================

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity =
            Mathf.Clamp(
                sensitivity,
                0.1f,
                20f
            );
    }

    public float GetMouseSensitivity()
    {
        return mouseSensitivity;
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();

        if (fovCoroutine != null)
        {
            StopCoroutine(fovCoroutine);
            fovCoroutine = null;
        }
    }
}