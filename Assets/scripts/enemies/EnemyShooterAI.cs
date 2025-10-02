using UnityEngine;

// Obliga a que el GameObject tenga Rigidbody2D y EnemyHealth
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyShooterAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;                // Velocidad de movimiento
    public float preferredDistance = 5f;        // Distancia preferida al jugador
    public float retreatDistance = 3f;          // Distancia para empezar a retroceder
    public float acceleration = 3f;             // Qué tan rápido acelera el enemigo

    [Header("Shooting")]
    public GameObject bulletPrefab;             // Prefab de la bala
    public float bulletSpeed = 10f;             // Velocidad de la bala (no usado aquí, pero útil)
    public float fireCooldown = 1.5f;           // Tiempo entre disparos

    [Header("Visuals / Animation")]
    public Animator animator;                   // Referencia al Animator, asignar en el Inspector
    private Vector2 lastAimDir = Vector2.right; // Última dirección de apuntado

    public bool isActive = false;               // Si el enemigo está activo y persiguiendo/disparando al jugador
    private Rigidbody2D rb;                     // Referencia al Rigidbody2D
    private Transform Target;                   // Referencia al objetivo (jugador)
    private float lastFireTime;                 // Última vez que disparó
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            Target = playerObj.transform; // Busca al jugador por tag

        RoomController room = GetComponentInParent<RoomController>();
        if (room != null)
            room.RegisterEnemy(gameObject); // Registra el enemigo en la sala
    }

    // Activa el enemigo y le asigna el objetivo
    public void Activate(Transform player)
    {
        Target = player;
        isActive = true;
    }

    // Se llama en cada frame de física
    private void FixedUpdate()
    {
        if (Target == null) return; // Si no hay objetivo, no hace nada
        if (!isActive || Target == null) return; // Si no está activo, no hace nada

        Vector2 toPlayer = Target.position - transform.position; // Vector hacia el jugador
        float distance = toPlayer.magnitude;                    // Distancia al jugador
        Vector2 aimDir = toPlayer.normalized;                   // Dirección normalizada hacia el jugador
        lastAimDir = aimDir;                                    // Guarda la última dirección

        // Mantener una cierta distancia del jugador
        Vector2 desiredVel = Vector2.zero;
        if (distance > preferredDistance)
        {
            desiredVel = toPlayer.normalized * moveSpeed; // Se acerca si está lejos
        }
        else if (distance > retreatDistance)
        {
            desiredVel = -toPlayer.normalized * moveSpeed; // Se aleja si está demasiado cerca
        }
        // Ajusta la velocidad suavemente
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desiredVel, acceleration * Time.fixedDeltaTime);

        // Gira para mirar al jugador
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        rb.rotation = angle;

        // Actualiza parámetros del Animator para la animación
        if (animator != null)
        {
            animator.SetFloat("Horizontal", aimDir.x);
            animator.SetFloat("Vertical",   aimDir.y);
        }

        // Dispara si el cooldown ha pasado y está a distancia adecuada
        if (Time.time - lastFireTime >= fireCooldown && distance <= preferredDistance + 2f)
        {
            FireBullet(toPlayer.normalized);
            lastFireTime = Time.time;
        }
    }

    // Instancia y dispara una bala en la dirección indicada
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