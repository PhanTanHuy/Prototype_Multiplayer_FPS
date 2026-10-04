using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [SerializeField] private int damage = 100;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        HitBox hitBox = other.GetComponentInChildren<HitBox>();
        if (hitBox != null)
        {
            Vector3 pos = other.ClosestPoint(transform.position);
            Quaternion rot = Quaternion.LookRotation(transform.forward);

            hitBox.TakeDame(damage, transform, pos, rot);
        }
    }
}