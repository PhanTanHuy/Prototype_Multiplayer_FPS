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

        float shake = CalculateShake(explosionPosition);

        if (shake > 0f)
        {
            CameraHolder.instance.cameraShake.Shake(0.75f, shake);
        }

        DetectHitBoxes(explosionPosition);
    }
    private float CalculateShake(Vector3 explosionPosition)
    {
        Transform cameraTransform = CameraHolder.instance.transform;

        float distance = Vector3.Distance(
            cameraTransform.position,
            explosionPosition
        );

        float maxDistance = 20f;
        float maxShake = 10f;

        float normalizedDistance = Mathf.Clamp01(
            distance / maxDistance
        );

        float shake = Mathf.Lerp(
            maxShake,
            0f,
            normalizedDistance
        );

        return shake;
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