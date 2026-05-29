using UnityEngine;
using System.Collections.Generic;

public class AttackBox : MonoBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask hitMask;

    BoxCollider box;

    HashSet<HitBox> hitTargets = new HashSet<HitBox>();

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.enabled = false;
        enabled = false;
    }

    void OnEnable()
    {
        hitTargets.Clear();
    }

    void Update()
    {
        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 halfExtents = box.size * 0.5f;
        Quaternion rotation = box.transform.rotation;


        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            rotation,
            hitMask
        );

        foreach (Collider col in hits)
        {
            if (col.TryGetComponent<HitBox>(out HitBox hitBox))
            {
                if (hitTargets.Contains(hitBox)) continue;

                hitTargets.Add(hitBox);
                Vector3 hitPoint = col.ClosestPoint(center);
                Quaternion hitRot = Quaternion.LookRotation(transform.forward);
                hitBox.TakeDame(damage, this.transform, hitPoint, hitRot);

              

            }
        }
    }
    void OnDrawGizmos()
    {
        if (!box) box = GetComponent<BoxCollider>();

        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(
            box.transform.TransformPoint(box.center),
            box.transform.rotation,
            box.transform.lossyScale
        );
        Gizmos.DrawWireCube(Vector3.zero, box.size);
    }


    public void OnDisable()
    {
        box.enabled = false;
    }
}
