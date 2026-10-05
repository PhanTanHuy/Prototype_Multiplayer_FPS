using UnityEngine;

public class NameBillboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (CameraHolder.instance == null)
            return;

        Vector3 direction =
            CameraHolder.instance.cameramain.transform.position
            - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}