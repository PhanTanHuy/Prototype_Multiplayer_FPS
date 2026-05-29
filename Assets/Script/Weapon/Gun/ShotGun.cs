using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShotGun : Gun
{
    [SerializeField] private int castPerShot;
    [SerializeField] private float spreadAngle = 5f;
    public override bool Shoot(float aimValue)
    {
        if (!base.Shoot(aimValue)) return false;

        BoltAction();

        RaycastHit hit;
        Vector3 camPos = camTransform.position + camTransform.forward * 0.3f;

        for (int i = 0; i < castPerShot; i++)
        {
            Vector3 shootDir = GetShotgunDirection(camTransform.forward, spreadAngle);

            if (Physics.Raycast(camPos, shootDir, out hit, 300f))
            {
                PoolObject.Instance.CreatBullet(firePosition.position, hit.point - firePosition.position, force, damage, rootParent);
            }
            else
            {
                PoolObject.Instance.CreatBullet(firePosition.position, shootDir, force, damage, rootParent);
            }
        }

        return true;
    }

    Vector3 GetShotgunDirection(Vector3 forward, float angle)
    {

        Quaternion rot = Quaternion.Euler(Random.Range(-angle, angle), Random.Range(-angle, angle), Random.Range(-angle, angle));
        return rot * forward;
    }

    public override void BoltAction()
    {
        if (boltActionCoroutine != null) StopCoroutine(boltActionCoroutine);
        boltActionCoroutine = StartCoroutine(IEBoltAction());
    }
    IEnumerator IEBoltAction()
    {
        float timeBoltAction = 0f;
        float timeDoneAction = 0.5f;
       
        while (timeBoltAction < timeDoneAction)
        {
            if (timeBoltAction > timeDoneAction / 2f)
            {
                bolt.localPosition = Vector3.Lerp(localEndBoltPosition, localOriginBoltPosition, (timeBoltAction - timeDoneAction / 2f) / (timeDoneAction / 2f));
            }
            else
            {
                bolt.localPosition = Vector3.Lerp(localOriginBoltPosition, localEndBoltPosition, timeBoltAction / (timeDoneAction / 2f));
            }
            timeBoltAction += Time.deltaTime;
            yield return null;
        }
        PoolObject.Instance.CreatShell(shellEjectPosition.position, shellEjectPosition.forward);
        bolt.localPosition = localOriginBoltPosition;
    }
}
