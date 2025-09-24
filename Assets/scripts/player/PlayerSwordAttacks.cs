using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
public class PlayerSwordAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject swordHitbox;
    private Collider2D swordCollider;
    public Animator animator; // Now references the SpriteObject's animator
    public Transform aimPivot; // Reference to the AimPivot

    [Header("Attack Settings")]
    public float attackDuration = 0.3f;
    public float attackDistance = 1f;
    [Header("Upgrades")]
    public bool canReflectBullets = false;

    private bool attacking = false;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        swordHitbox.SetActive(false);
        swordCollider = swordHitbox.GetComponent<Collider2D>();
        swordCollider.enabled = false;
        
        playerInput = GetComponent<PlayerInput>();

        // Access the "Attack" action (must exist in Input Actions asset)
        if (playerInput != null)
        {
        attackAction = playerInput.actions["Attack"];
        if (attackAction == null)
        {
            Debug.LogError("Attack action not found in Input Actions asset!");
        }
     }
        else
        {
        Debug.LogError("PlayerInput component not found on Player!");
        }
    }

    void Update()
    {
        if (attacking) return;

        //  Mouse input
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(SwingSword());
        }
        //  Controller input (A button, etc. mapped in Input Actions)
        else if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            StartCoroutine(SwingSword());
        }
        //  Keyboard input (manual key)
        else if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            StartCoroutine(SwingSword());
        }
    }

    private System.Collections.IEnumerator SwingSword()
    {
        attacking = true;
        swordCollider.enabled = true;
        swordHitbox.SetActive(true);
        // Place the hitbox fixed in front of the player
        PositionSwordHitbox();

        // Keep attack window open
        yield return new WaitForSeconds(attackDuration);
        swordHitbox.SetActive(false);
        swordCollider.enabled = false;
        attacking = false;
    }

    private void PositionSwordHitbox()
    {
        if (aimPivot == null) return;
        
        Vector2 attackDirection = aimPivot.right;
        swordHitbox.transform.position = aimPivot.position + (Vector3)(attackDirection * attackDistance);

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        swordHitbox.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private Vector2 GetAimDirection()
    {
        return aimPivot.right;
    }

    // This detects when sword hits something

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!attacking) return;

        // damage enemies
        Health target = other.GetComponent<Health>();
        if (target != null && other.CompareTag("Enemy"))
        {
            target.TakeDamage(1);
        }

        // bullet reflection
        if (other.CompareTag("EnemyBullet"))
        {
            if (canReflectBullets)
            {
                Rigidbody2D brb = other.GetComponent<Rigidbody2D>();
                if (brb != null)
                {
                    Vector2 aimDir = GetAimDirection();
                    other.tag = "playerBullet";

                    //retargeting
                    EnemyBullet eb = other.GetComponent<EnemyBullet>();
                    if (eb != null) eb.Redirect(aimDir);
                }

            }
            else
            {
                // destroy bullet if not reflecting
                Destroy(other.gameObject);
            }

        }
    }
}
