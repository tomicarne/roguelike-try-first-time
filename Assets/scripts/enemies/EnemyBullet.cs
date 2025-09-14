using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public int damage = 1;
    public float speed = 15f;
    public float lifetime = 2f;

    private Rigidbody2D rb;
    public void Initialize(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;
        // rotate sprite
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && tag == "EnemyBullet")
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Enemy") && tag == "playerBullet")
        {
            EnemyHealth eh = other.GetComponent<EnemyHealth>();
            if (eh != null) eh.TakeDamage(damage);
            Destroy(gameObject);
        }

    }

    public void Redirect(Vector2 newDirection)
    {
        rb.linearVelocity = newDirection.normalized * speed;

        // rotate sprite
        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }
}
