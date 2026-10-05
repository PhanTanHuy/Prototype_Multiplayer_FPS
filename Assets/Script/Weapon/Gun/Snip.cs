using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Snip : Gun
{
    public GameObject CameraSnip;
    public override void SetLocalWeapon(bool isMine)
    {
        if (!isMine) return;
        CameraSnip.SetActive(true);
    }
    public override bool Shoot(float aimValue)
    {
        if (!base.Shoot(aimValue)) return false;

        BoltAction();
        if (Physics.Raycast(camTransform.position + camTransform.forward * 0.3f, camTransform.forward, out RaycastHit hit, distanceRaycast, hitBoxLayer))
        {
            OnHit(hit, camTransform.forward);
        }
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
