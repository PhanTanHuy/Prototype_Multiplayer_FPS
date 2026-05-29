using UnityEngine;

public class GunLaser : MonoBehaviour
{
    public Transform firePosition;
    private Vector3 laserEnd;
    private LineRenderer lineRenderer;
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
    }
    private void Update()
    {
        lineRenderer.SetPosition(0, firePosition.position);
        laserEnd = firePosition.position + firePosition.forward * -1f * 100f;
        lineRenderer.SetPosition(1, laserEnd);
    }
}
