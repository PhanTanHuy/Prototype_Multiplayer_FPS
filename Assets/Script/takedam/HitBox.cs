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
            { HitBoxTag.Head, 10 }
        };
    public void TakeDame(int damage, Transform ori, Vector3 pos, Quaternion rot)
    {
        PoolObject.Instance.CreatBlood(pos, rot);
        int multi = damageMultiplier[hitBoxTag];
        damage *= multi;
        if (hitBoxTag == HitBoxTag.Head) PlayerUI.instance.SetTextHitHeadShot(damage);
        else PlayerUI.instance.SetTextHitNormal(damage);
        //healthManager.TakeDame(multi * damage);
        healthManager.SendTakeDamage(damage);
        //if (ori.root == this.transform.root) return;
      
    }
}
