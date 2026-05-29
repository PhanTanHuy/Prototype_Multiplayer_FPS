using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private Rigidbody rb;
    public void SetBombProjectile(Vector3 direction)
    {
        Invoke("Explosion", 3f);
        rb.transform.localPosition = Vector3.zero;
        rb.gameObject.SetActive(true);
        rb.AddForce(direction * 10f, ForceMode.Impulse);
    }
    private void Explosion()
    {
        rb.gameObject.SetActive(false);
        explosionParticle.transform.position = rb.transform.position;
        explosionParticle.Play();
    }
    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
