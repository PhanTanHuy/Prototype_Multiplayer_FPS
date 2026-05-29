using UnityEngine;

public class Shell : MonoBehaviour
{
    [Header("Force")]
    [SerializeField] float ejectForce = 2.5f;
    [SerializeField] float upForce = 1.2f;
    [SerializeField] float gravity = 9.8f;

    [Header("Rotation")]
    [SerializeField] Vector3 spinSpeed = new Vector3(600, 300, 400);

    [Header("Life")]
    [SerializeField] float lifeTime = 5f;

    Vector3 velocity;
    float timer;
    bool stopped;

    void OnEnable()
    {
        timer = 0f;
        stopped = false;
    }

    public void SetShell(Vector3 direction)
    {
        velocity =
            direction.normalized * ejectForce +
            Vector3.up * upForce;

        spinSpeed = Random.insideUnitSphere * 600f;
        transform.rotation =
        Quaternion.LookRotation(direction) *
        Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        if (stopped) return;

        velocity.y -= gravity * Time.deltaTime;

        transform.position += velocity * Time.deltaTime;

        transform.Rotate(spinSpeed * Time.deltaTime, Space.Self);

        if (transform.localPosition.y <= 0.02f)
        {
            stopped = true;
            velocity = Vector3.zero;
        }

        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }
}
