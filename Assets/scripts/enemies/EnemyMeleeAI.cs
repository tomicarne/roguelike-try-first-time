using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyMeleeAI : MonoBehaviour
{
    [Header("movement")]
    public float moveSpeed = 3f;           // Velocidad de movimiento del enemigo
    public float stoppingDistance = 0.01f; // Distancia mínima para dejar de moverse hacia el objetivo
    public float acceleration = 5f;        // Qué tan rápido acelera el enemigo
    public bool isActive = false;          // Si el enemigo está activo y persiguiendo al jugador

    [Header("Visuals / Animation")]
    public Animator animator;              // Referencia al Animator, asignar en el Inspector
    private Vector2 lastAimDir = Vector2.right; // Última dirección de movimiento

    private Transform Target;              // Referencia al objetivo (jugador)
    private Rigidbody2D rb;                // Referencia al Rigidbody2D

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Se llama al crear el objeto, inicializa referencias
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Target = playerObj.transform; // Busca al jugador por tag
        }
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
    void FixedUpdate()
    {
        if (Target == null) return; // Si no hay objetivo, no hace nada
        if (!isActive || Target == null) return; // Si no está activo, no hace nada

        Vector2 dir = Target.position - transform.position; // Calcula dirección al objetivo
        float distance = dir.magnitude;                     // Calcula distancia al objetivo

        Vector2 aimDir = dir.normalized; // Normaliza la dirección
        lastAimDir = aimDir;             // Guarda la última dirección

        // Actualiza parámetros del Animator para la animación
        if (animator != null)
        {
            animator.SetFloat("Horizontal", aimDir.x);
            animator.SetFloat("Vertical",   aimDir.y);
        }

        // Si está lejos del objetivo, se mueve hacia él
        if (distance > stoppingDistance)
        {
            Vector2 desiredVel = dir.normalized * moveSpeed;
            float acc = acceleration * Time.fixedDeltaTime;

            rb.linearVelocity = Vector2.Lerp(
                rb.linearVelocity,
                desiredVel,
                acc);
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Si está cerca, se detiene
        }
    }

}

