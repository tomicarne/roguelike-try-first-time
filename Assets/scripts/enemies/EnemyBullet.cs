using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    //daño hecho al jugador
    public int damage = 1;
    // velocidad de proyectil
    public float speed = 15f;
    // teimpo de vida de proyectil
    public float lifetime = 2f;
    //obtener el rigid body de la bala
    private Rigidbody2D rb;
    public void Initialize(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;
        // girar el sprite para que sea apuntado a donde se disparo la bala
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }

    void Start()
    {
        //obtener los componentes necesarios
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        // si se sobrepasa la lifetime el projectil es destruido
        Destroy(gameObject, lifetime);
    }
    //en collision dependiendo de las tags si se golpea al jugador o enemigo se baja la HP accediendo a Health.cs
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
    // se redirecciona la bala a una nueva direccion

    public void Redirect(Vector2 newDirection)
    {
        rb.linearVelocity = newDirection.normalized * speed;

        // rotate sprite
        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }
}
