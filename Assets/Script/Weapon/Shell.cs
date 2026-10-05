using UnityEngine;

public class Shell : MonoBehaviour
{
    [Header("Force")]
    [SerializeField] private float ejectForce = 2.5f;
    [SerializeField] private float upForce = 1.2f;
    [SerializeField] private float gravity = 9.8f;

    [Header("Collision")]
    [SerializeField] private LayerMask staticMapLayer;
    [SerializeField] private float collisionRadius = 0.02f;

    [Header("Rotation")]
    [SerializeField] private Vector3 spinSpeed = new Vector3(600, 300, 400);

    [Header("Life")]
    [SerializeField] private float lifeTime = 5f;

    private Vector3 velocity;
    private float timer;
    private bool stopped;

    private void OnEnable()
    {
        timer = 0f;
        stopped = false;
        velocity = Vector3.zero;
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

    private void Update()
    {
        if (stopped)
            return;

        float deltaTime = Time.deltaTime;

        // Gravity
        velocity.y -= gravity * deltaTime;

        Vector3 movement = velocity * deltaTime;

        // Kiểm tra collision trước khi di chuyển
        float distance = movement.magnitude;

        if (distance > 0f)
        {
            if (Physics.SphereCast(
                transform.position,
                collisionRadius,
                movement.normalized,
                out RaycastHit hit,
                distance,
                staticMapLayer,
                QueryTriggerInteraction.Ignore))
            {
                // Đặt shell sát mặt đất/tường
                transform.position =
                    hit.point + hit.normal * collisionRadius;

                StopShell();

                return;
            }
        }

        // Di chuyển
        transform.position += movement;

        // Xoay
        transform.Rotate(
            spinSpeed * deltaTime,
            Space.Self
        );

        // Lifetime
        timer += deltaTime;

        if (timer >= lifeTime)
        {
            gameObject.SetActive(false);
        }
    }

    private void StopShell()
    {
        stopped = true;
        velocity = Vector3.zero;
    }
}