using System.Collections;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{

    [Header("Sway Settings")]
    [SerializeField] private float smooth;
    [SerializeField] private float multiplier;
    [HideInInspector] public Transform targetSway;
    private InputSystemActions inputActions;
    private Vector2 mouseInput;
    private void Start()
    {
        inputActions = new InputSystemActions();
        inputActions.Enable();
        inputActions.Player.Look.performed += ctx => mouseInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => mouseInput = Vector2.zero;
    }
    public void StopSway()
    {
        if (targetSway == null) return; 
        StartCoroutine(IEStopSway());
        enabled = false;
    }
    IEnumerator IEStopSway()
    {
        while (Quaternion.Angle(targetSway.localRotation, Quaternion.identity) > 0.001f)
        {
            targetSway.localRotation = Quaternion.Slerp(targetSway.localRotation, Quaternion.identity, smooth * Time.deltaTime);
            yield return null;
        }
        targetSway.localRotation = Quaternion.identity;
    }
    public void StartSway()
    {
        enabled = true;
    }
    private void Update()
    {
        if (targetSway == null) return;
        float mouseX = mouseInput.x * multiplier;
        float mouseY = mouseInput.y * multiplier;

        Quaternion rotationX = Quaternion.AngleAxis(-mouseY, Vector3.right);
        Quaternion rotationY = Quaternion.AngleAxis(mouseX, Vector3.up);

        Quaternion targetRotation = rotationX * rotationY;

        targetSway.localRotation = Quaternion.Slerp(targetSway.localRotation, targetRotation, smooth * Time.deltaTime);
    }
}