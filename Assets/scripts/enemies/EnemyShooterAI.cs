using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyShooterAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float preferredDistance = 5f;
    public float retreatDistance = 3f;
    public float acceleration = 3f;
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireCooldown = 1.5f;

    private Rigidbody2D rb;
    private Transform Target;
    private float lastFireTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            Target = playerObj.transform;
    }

    private void FixedUpdate()
    {
        if (Target == null) return;

        Vector2 toPlayer = Target.position - transform.position;
        float distance = toPlayer.magnitude;

        // mantener una cierta distancia
        Vector2 desiredVel = Vector2.zero;
        if (distance > preferredDistance)
        {
            desiredVel = toPlayer.normalized * moveSpeed;
        }
        else if (distance > retreatDistance)
        {
            desiredVel = -toPlayer.normalized * moveSpeed;
        }
        // ajustar la velocidad
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, acceleration * Time.fixedDeltaTime);

        //mirar al jugador
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        rb.rotation = angle;

        // disparar si es posible
        if (Time.time - lastFireTime >= fireCooldown && distance <= preferredDistance + 2f)
        {
            FireBullet(toPlayer.normalized);
            lastFireTime = Time.time;
        }
    }
    private void FireBullet(Vector2 direction)
    {

        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        EnemyBullet bulletRb = bulletObj.GetComponent<EnemyBullet>();
        if (bulletRb != null)
        {
            bulletRb.Initialize(direction);
        }
    }
}
