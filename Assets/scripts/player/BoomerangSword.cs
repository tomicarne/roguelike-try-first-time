using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangSword : MonoBehaviour
{
    [Header("Boomerang Settings")]
    public float throwSpeed = 10f;
    public float maxDistance = 6f;
    public bool canBounceUpgrade = false;   // Set true when player unlocks the bounce upgrade
    public float bounceRadius = 5f;
    public int damage = 1;  

    private Transform player;
    private bool returning = false;
    private Vector3 origin;
    private Rigidbody2D rb;
    private PlayerSwordAttack owner; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

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
        if (!returning && Vector3.Distance(origin, transform.position) >= maxDistance)
        {
            ReturnToPlayer();
        }
        if (returning)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * throwSpeed;

            // if close enough, re-attach to player
            if (Vector2.Distance(transform.position, player.position) < 0.5f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                gameObject.SetActive(false); // hide sword until next throw

                if (owner) owner.isThrowing = false;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!returning && other.CompareTag("Enemy"))
        {
            // normal damage handled by PlayerSwordAttack OnTriggerEnter2D
            Health hp = other.GetComponent<Health>();
            if (hp) hp.TakeDamage(damage);
            
            if (canBounceUpgrade)
            {
                // look for another enemy nearby to bounce to
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

            // no bounce target → return to player
            ReturnToPlayer();
        }
    }
    private void ReturnToPlayer()
    {
        returning = true;
    }
}
