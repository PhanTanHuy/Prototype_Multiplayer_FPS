using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BombProjectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private Rigidbody rb;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int damage = 200;
    [SerializeField] private LayerMask hitBoxLayer;

    public void SetBombProjectile(Vector3 direction)
    {
        Invoke(nameof(Explosion), 3f);

        rb.transform.localPosition = Vector3.zero;
        rb.gameObject.SetActive(true);

        rb.AddForce(direction * 10f, ForceMode.Impulse);
    }

    private void Explosion()
    {
        Vector3 explosionPosition = rb.transform.position;

        rb.gameObject.SetActive(false);

        explosionParticle.transform.position = explosionPosition;
        explosionParticle.Play();

        DetectHitBoxes(explosionPosition);
    }

    private void DetectHitBoxes(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(
            position,
            explosionRadius,
            hitBoxLayer
        );

        HashSet<Transform> damagedPlayers = new();

        foreach (Collider collider in colliders)
        {
            HitBox hitBox = collider.GetComponent<HitBox>();

            if (hitBox == null)
                continue;

            Transform playerRoot = hitBox.transform.root;
            if (playerRoot.GetComponent<PhotonView>().IsMine) return;
            if (!damagedPlayers.Add(playerRoot))
                continue;

            hitBox.TakeDame(
                damage,
                transform,
                position,
                Quaternion.identity
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}