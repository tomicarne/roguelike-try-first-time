using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    [Range(0f, 1f)] public float steeringFactor = 0.2f; // 0 = locked, 1 = fully steerable

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction dashAction;
    private InputAction moveAction;
    private PlayerMovement movementScript;

    private bool isDashing = false;
    private bool canDash = true;
    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        movementScript = GetComponent<PlayerMovement>();

        if (playerInput != null)
        {
            var actions = playerInput.actions;
            if (actions != null)
            {
                dashAction = actions["Dash"];
                moveAction = actions["Move"];
            }
        }
    }

    private void Update()
    {
        if (!canDash || dashAction == null) return;

        if (dashAction.WasPressedThisFrame())
        {
            Vector2 startDir = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            if (startDir.sqrMagnitude < 0.0001f)
                startDir = movementScript != null ? movementScript.GetLastMoveDirection() : Vector2.right;

            StartCoroutine(DashRoutine(startDir)); // <-- name & param must match
        }
    }
    private System.Collections.IEnumerator DashRoutine(Vector2 startDirection)
    {
        canDash = false;
        isDashing = true;
        // make player invincible during dash
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.SetTemporaryInvincibility(dashDuration);
        }

        Vector2 dashBaseDir = startDirection.normalized;
        float elapsed = 0f;

        // save original scale
        Vector3 originalScale = transform.localScale;

        while (elapsed < dashDuration)
        {
            Vector2 steer = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector2 finalDir = ((1f - steeringFactor) * dashBaseDir + steeringFactor * steer).normalized;

            rb.linearVelocity = finalDir * dashSpeed;

            // stretch effect
            float stretchAmount = 1.5f; // adjust for more/less stretch
            float squashAmount = 0.7f; // adjust for more/less squash
            float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            transform.localScale = new Vector3(originalScale.x * stretchAmount, originalScale.y * squashAmount, originalScale.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        transform.localScale = originalScale;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

}
