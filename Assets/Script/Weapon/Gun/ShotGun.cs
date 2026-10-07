using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : Gun
{
    [SerializeField] private int castPerShot;
    [SerializeField] private float spreadAngle = 5f;
    public override bool Shoot(float aimValue)
    {
        if (!base.Shoot(aimValue)) return false;
        BoltAction();
        for (int i = 0; i < castPerShot; i++)
        {
            Vector3 shootDir = GetShotgunDirection(camTransform.forward, spreadAngle);

            if (Physics.Raycast(camTransform.position + camTransform.forward * 0.3f, shootDir, out RaycastHit hit, distanceRaycast, hitBoxLayer))
            {
                OnHit(hit, camTransform.forward);
            }
        }
        return true;
    }
    public void OnHitShotGun(RaycastHit hit, Vector3 moveDir)
    {
        Vector3 pos = hit.point + hit.normal * 0.01f;
        Quaternion rot = Quaternion.LookRotation(hit.normal);
        if (hit.collider.TryGetComponent<HitBox>(out HitBox target))
        {
            target.TakeDame(damage, this.transform, pos, rot);
            PlayerUI.instance.HitSignal();
        }
        else
        {
            PoolObject.Instance.CreatBulletHole(pos, rot);
        }
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
