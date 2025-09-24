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

    [Header("Visual References")]
    public Transform visuals;
    public Transform aimPivot;

    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction dashAction;
    private InputAction moveAction;
    private PlayerMovement movementScript;

    private bool isDashing = false;
    private bool canDash = true;
    public bool IsDashing => isDashing;

    private Vector3 originalVisualsScale;
    private Vector3 originalAimPivotScale;

    private void Awake()
    {
        // Cache components
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        movementScript = GetComponent<PlayerMovement>();
        // Setup input actions
        if (playerInput != null)
        {
            var actions = playerInput.actions;
            if (actions != null)
            {
                dashAction = actions["Dash"];
                moveAction = actions["Move"];
            }
        }
        if (visuals != null) originalVisualsScale = visuals.localScale;
        if (aimPivot != null) originalAimPivotScale = aimPivot.localScale;
    }

    private void Update()
    {
        // Handle dash input
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
        // start dash
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
        while (elapsed < dashDuration)
        {
            // Calcular dirección final del dash con steering
            Vector2 steer = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector2 finalDir = ((1f - steeringFactor) * dashBaseDir + steeringFactor * steer).normalized;

            // Mover el Rigidbody (física)
            rb.linearVelocity = finalDir * dashSpeed;

            // Aplicar efectos visuales SOLO en el GameObject Visuals
            ApplyDashVisualEffects(finalDir);

            elapsed += Time.deltaTime;
            yield return null;
        }
        // end dash
        rb.linearVelocity = Vector2.zero;
        ResetDashVisualEffects();
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private void ApplyDashVisualEffects(Vector2 dashDirection)
    {
        if (visuals == null) return;

        // Efecto de estiramiento
        float stretchAmount = 1.5f;
        float squashAmount = 0.5f;

        // Calcular la escala basada en la dirección del dash
        Vector3 dashScale = new Vector3(
            originalVisualsScale.x * stretchAmount,
            originalVisualsScale.y * squashAmount,
            originalVisualsScale.z
        );

        // Aplicar escala al Visuals
        visuals.localScale = dashScale;

        // Rotar el aimPivot para que apunte en la dirección del dash
        if (aimPivot != null)
        {
            float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    private void ResetDashVisualEffects()
    {
        // Resetear escala del Visuals
        if (visuals != null)
            visuals.localScale = originalVisualsScale;

        // El aimPivot se reseteará automáticamente por PlayerAiming en el próximo Update
    }
    public void InterruptDash()
    {
        if (isDashing)
        {
            StopAllCoroutines();
            rb.linearVelocity = Vector2.zero;
            ResetDashVisualEffects();
            isDashing = false;
            canDash = true;
        }
    }

}
