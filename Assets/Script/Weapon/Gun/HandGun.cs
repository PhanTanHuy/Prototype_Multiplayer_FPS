using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HandGun : Gun
{
    [SerializeField] private bool automaticHandGun;
    public override bool Shoot(float aimValue)
    {
        if (!base.Shoot(aimValue)) return false;
        this.BoltAction();
        RaycastHit hitPointFormCamera;
        if (Physics.Raycast(camTransform.position + camTransform.forward * 0.3f, camTransform.forward, out hitPointFormCamera, 1000f))
        {
            PoolObject.Instance.CreatBullet(firePosition.position, hitPointFormCamera.point - firePosition.position, force, damage, rootParent);
        }
        else PoolObject.Instance.CreatBullet(firePosition.position, firePosition.forward, force, damage, rootParent);
        return true;
    }
    public override void BoltAction()
    {
        if (boltActionCoroutine != null) StopCoroutine(boltActionCoroutine);
        boltActionCoroutine = StartCoroutine(IEBoltAction());
    }
    IEnumerator IEBoltAction()
    {
        float timeBoltAction = 0f;
        float timeDoneAction = 0.1f;
        PoolObject.Instance.CreatShell(shellEjectPosition.position, shellEjectPosition.forward);
        if (currentAmmo == 0)
        {
            while (timeBoltAction < timeDoneAction / 2f)
            {
                Debug.Log("a");
                bolt.localPosition = Vector3.Lerp(localOriginBoltPosition, localEndBoltPosition, timeBoltAction / (timeDoneAction / 2f));
                timeBoltAction += Time.deltaTime;
                yield return null;
            }
            Debug.Log("b");
            bolt.localPosition = localEndBoltPosition;
        }
        else
        {
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
            bolt.localPosition = localOriginBoltPosition;
        }
    }
}
