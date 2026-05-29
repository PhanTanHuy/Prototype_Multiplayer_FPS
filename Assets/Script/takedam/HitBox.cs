using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    [SerializeField] private HealthManager healthManager;

    public enum HitBoxTag
    {
        Leg,
        Spine,
        Head
    }

    [SerializeField] private HitBoxTag hitBoxTag;

    private static readonly Dictionary<HitBoxTag, int> damageMultiplier =
        new Dictionary<HitBoxTag, int>
        {
            { HitBoxTag.Leg, 1 },
            { HitBoxTag.Spine, 2 },
            { HitBoxTag.Head, 5 }
        };
    public void TakeDame(int damage, Transform ori, Vector3 pos, Quaternion rot)
    {
        if (ori.root == this.transform.root) return;
        PoolObject.Instance.CreatBlood(pos, rot);
        int multi = damageMultiplier[hitBoxTag];
        healthManager.TakeDame(multi * damage);
    }
}
