using UnityEngine;

public class Bullet : MonoBehaviour
{
    Vector3 velocity;
    Vector3 lastPos;
    int damage;
    [SerializeField] private LayerMask hitMask;

    void OnEnable()
    {
        lastPos = transform.position;
    }

    void Update()
    {
        Vector3 currentPos = transform.position;

        velocity *= 0.995f;
        velocity += Vector3.down * 9.8f * Time.deltaTime;

        Vector3 nextPos = currentPos + velocity * Time.deltaTime;
        Vector3 move = nextPos - currentPos;

        float distance = move.magnitude;

        if (Physics.Raycast(currentPos, move.normalized, out RaycastHit hit, distance, hitMask))
        {
            OnHit(hit, move.normalized);
            return;
        }

        transform.position = nextPos;
        lastPos = nextPos;
    }


    void OnHit(RaycastHit hit, Vector3 moveDir)
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

        gameObject.SetActive(false);
    }

    public void SetBullet(Vector3 dir, float spd, int d)
    {
        velocity = dir.normalized * spd;
        damage = d;
    }
}
