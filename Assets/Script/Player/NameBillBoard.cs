using UnityEngine;

public class NameBillboard : MonoBehaviour
{
    private Camera targetCamera;

    public void SetCamera(Camera camera)
    {
        targetCamera = camera;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        transform.LookAt(targetCamera.transform);
        transform.Rotate(0f, 180f, 0f);
    }
}