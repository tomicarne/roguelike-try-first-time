using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyMeleeAI : MonoBehaviour
{
    [Header("movement")]
    public float moveSpeed = 3f;
    public float stoppingDistance = 1f;
    public float acceleration = 5f;

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
    }

    void FixedUpdate()
    {
        if (Target == null) return;

        Vector2 dir = Target.position - transform.position;
        float distance = dir.magnitude;

        if (distance > stoppingDistance)
        {
            Vector2 desiredVel = dir.normalized * moveSpeed;
            float acc = acceleration* Time.fixedDeltaTime;

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

