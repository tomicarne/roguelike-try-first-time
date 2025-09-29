using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyMeleeAI : MonoBehaviour
{
    [Header("movement")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.01f;
    public float acceleration = 5f;
    public bool isActive = false;
    [Header("Visuals / Animation")]
    public Animator animator;       // assign in Inspector
    private Vector2 lastAimDir = Vector2.right;


    private Transform Target;
    private Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Target = playerObj.transform;
        }
        RoomController room = GetComponentInParent<RoomController>();
            if (room != null)
        room.RegisterEnemy(gameObject);
    }
    public void Activate(Transform player)
    {
        Target = player;
        isActive = true;
    }

    void FixedUpdate()
    {
        if (Target == null) return;
        if (!isActive || Target == null) return;
        
        Vector2 dir = Target.position - transform.position;
        float distance = dir.magnitude;
        
        Vector2 aimDir = dir.normalized;
        lastAimDir = aimDir; // keep for idle

        if (animator != null)
        {
            animator.SetFloat("Horizontal", aimDir.x);
            animator.SetFloat("Vertical",   aimDir.y);
        }

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
            rb.linearVelocity = Vector2.zero;
        }
    }

}

