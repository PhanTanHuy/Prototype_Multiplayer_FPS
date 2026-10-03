using System.Collections;
using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [Header("Network Setting")]

    private Coroutine recoil;
    [SerializeField] private float mouseSensitivity = 5f;
    [SerializeField] private float maxLookUpAngle = 80f;
    [SerializeField] private Transform rootPlayer;
    [SerializeField] private CameraShake cameraShake;
    private float xRotation = 0f;
    private Vector2 mouseDelta;
    private InputSystemActions inputActions;
    private Coroutine viewCoroutine;
    private float defauleFieldOfView;

    [SerializeField] float mouseSmoothTime = 0.05f;

    private Vector2 currentMouseDelta;
    private Vector2 mouseDeltaVelocity;
    private float aimStateSensitivity = 1f;


    private void Start()
    {
            inputActions = new InputSystemActions();
            inputActions.Enable();
            inputActions.Player.Look.performed += ctx => mouseDelta = ctx.ReadValue<Vector2>();
            inputActions.Player.Look.canceled += ctx => mouseDelta = Vector2.zero;
            defauleFieldOfView = Camera.main.fieldOfView;
    }

    private void Update()
    {
            HandleCameraRotation();
    }

    public void SetAimSentivity(float vl)
    {
        aimStateSensitivity = vl;
    }

    public void ResetAimSentivity()
    {
        aimStateSensitivity = 1f;
    }

    private void HandleCameraRotation()
    {
        Vector2 rawMouseDelta = inputActions.Player.Look.ReadValue<Vector2>();
        rawMouseDelta /= aimStateSensitivity;

        // Smooth input chuột
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            rawMouseDelta,
            ref mouseDeltaVelocity,
            mouseSmoothTime
        );

        float mouseX = currentMouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = currentMouseDelta.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookUpAngle, maxLookUpAngle);

        // Xoay Camera Holder nội bộ
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Xoay trục Y của người
        rootPlayer.transform.Rotate(Vector3.up * mouseX);

    }

    // -------------------------------------------------------------
    // CHẶN HIỆU ỨNG THỊ GIÁC: Máy người khác không được giật FOV hay màn hình của mình
    // -------------------------------------------------------------

    public void SetViewAim()
    {
        if (viewCoroutine != null) StopCoroutine(viewCoroutine);
        viewCoroutine = StartCoroutine(IESetView(defauleFieldOfView - 10f));
    }

    IEnumerator IESetView(float target)
    {
        Camera cam = Camera.main;
        float speed = 50f;

        while (!Mathf.Approximately(cam.fieldOfView, target))
        {
            cam.fieldOfView = Mathf.MoveTowards(
                cam.fieldOfView,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }
        cam.fieldOfView = target;
    }

    public void SetViewDefault()
    {
        if (viewCoroutine != null) StopCoroutine(viewCoroutine);
        viewCoroutine = StartCoroutine(IESetView(defauleFieldOfView));
    }

    public void RecoilCamera(float recoilAmmount)
    {
        if (recoil != null) StopCoroutine(recoil);
        recoil = StartCoroutine(IERecoilCamera(recoilAmmount));
    }

    IEnumerator IERecoilCamera(float recoilAmmount)
    {
        yield return null;
        float xRotBe = xRotation;

        if (cameraShake != null) cameraShake.Shake(0.15f, -recoilAmmount / 3f);

        recoilAmmount /= aimStateSensitivity;
        float timeRecoil = 0f;
        float rotY = recoilAmmount * Random.Range(-1f, 1f) / 0.15f;
        recoilAmmount *= Random.Range(0.5f, 1f);
        while (timeRecoil < 0.15f)
        {
            xRotation += recoilAmmount * Time.deltaTime / 0.15f;
            rootPlayer.transform.Rotate(Vector3.up * rotY * Time.deltaTime);
            timeRecoil += Time.deltaTime;
            yield return null;
        }
    }

    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
    }
    /////////NETWORK FUNCTION
    public float GetNetworkPitch()
    {
        return xRotation;
    }
    public void SetNetworkPitch(float pitch)
    {
        xRotation = pitch;
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}