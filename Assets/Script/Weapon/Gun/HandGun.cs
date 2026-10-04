using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HandGun : Gun
{
    [SerializeField] private bool automaticHandGun;
    public override bool Shoot(float aimValue)
    {
        if (!base.Shoot(aimValue)) return false;
        this.BoltAction();
        if (Physics.Raycast(camTransform.position + camTransform.forward * 0.3f, camTransform.forward, out RaycastHit hit, distanceRaycast))
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
