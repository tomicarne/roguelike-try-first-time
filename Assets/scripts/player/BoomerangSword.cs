using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangSword : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public float throwSpeed = 10f;           // Velocidad de lanzamiento
    public float maxDistance = 6f;           // Distancia máxima antes de regresar
    public bool canBounceUpgrade = false;    // Si el jugador tiene la mejora de rebote
    public float bounceRadius = 5f;          // Radio para buscar enemigos al rebotar
    public int damage = 1;                   // Daño que inflige

    private Transform player;                // Referencia al jugador
    private bool returning = false;          // Si la espada está regresando al jugador
    private Vector3 origin;                  // Posición inicial del lanzamiento
    private Rigidbody2D rb;                  // Referencia al Rigidbody2D
    private PlayerSwordAttack owner;         // Referencia al script del jugador

    //inicializa el Rigidbody2D
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    // Lanza la espada en una dirección desde el jugador
    public void Throw(Transform playerTransform, PlayerSwordAttack playerScript, Vector2 direction)
    {
        player = playerTransform;
        owner = playerScript;
        origin = player.position;
        returning = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = direction.normalized * throwSpeed;
        if (owner) owner.isThrowing = true;
    }

    void Update()
    {
        // Si llegó a la distancia máxima, regresa al jugador
        if (!returning && Vector3.Distance(origin, transform.position) >= maxDistance)
        {
            ReturnToPlayer();
        }
        // Si está regresando, mueve la espada hacia el jugador
        if (returning)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * throwSpeed;

            // Si está lo suficientemente cerca del jugador, se desactiva
            if (Vector2.Distance(transform.position, player.position) < 0.5f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                gameObject.SetActive(false); // Oculta la espada hasta el próximo lanzamiento

                if (owner) owner.isThrowing = false;
            }
        }
    }

    // Detecta colisiones con enemigos
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!returning && other.CompareTag("Enemy"))
        {
            // Aplica daño al enemigo
            Health hp = other.GetComponent<Health>();
            if (hp) hp.TakeDamage(damage);
            
            if (canBounceUpgrade)
            {
                // Busca otro enemigo cercano para rebotar
                Collider2D nextEnemy = Physics2D.OverlapCircle(other.transform.position,
                                                               bounceRadius,
                                                               LayerMask.GetMask("Enemy"));
                if (nextEnemy && nextEnemy.gameObject != other.gameObject)
                {
                    Vector2 bounceDir = (nextEnemy.transform.position - transform.position).normalized;
                    rb.linearVelocity = bounceDir * throwSpeed;
                    return;
                }
            }

            // Si no hay rebote, regresa al jugador
            ReturnToPlayer();
        }
    }

    // Marca la espada para que regrese al jugador
    private void ReturnToPlayer()
    {
        returning = true;
    }
}