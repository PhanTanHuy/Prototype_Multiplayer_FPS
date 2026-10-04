using UnityEngine;

public class NameBillboard : MonoBehaviour
{


    private void LateUpdate()
    {
        if (CameraHolder.instance == null)
            return;

        Vector3 direction = transform.position - CameraHolder.instance.cameramain.transform.position;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}