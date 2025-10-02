using UnityEngine;
using UnityEngine.InputSystem;

// Requiere Rigidbody2D para el movimiento físico
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;           // Velocidad del dash
    public float dashDuration = 0.2f;       // Duración del dash en segundos
    public float dashCooldown = 1f;         // Tiempo de recarga entre dashes
    [Range(0f, 1f)] public float steeringFactor = 0.2f; // Permite controlar el dash durante el movimiento

    [Header("Visual References")]
    public Transform visuals;               // Referencia al objeto visual del jugador
    public Transform aimPivot;              // Referencia al pivote de apuntado

    private Rigidbody2D rb;                 // Referencia al Rigidbody2D
    private PlayerInput playerInput;        // Referencia al PlayerInput
    private InputAction dashAction;         // Acción de input para dash
    private InputAction moveAction;         // Acción de input para movimiento
    private PlayerMovement movementScript;  // Referencia al script de movimiento

    private bool isDashing = false;         // Si el jugador está haciendo dash
    private bool canDash = true;            // Si el dash está disponible
    public bool IsDashing => isDashing;     // Propiedad pública para consultar si está en dash

    private Vector3 originalVisualsScale;   // Escala original del objeto visual
    private Vector3 originalAimPivotScale;  // Escala original del aimPivot

    private void Awake()
    {
        // Cachea componentes necesarios
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        movementScript = GetComponent<PlayerMovement>();
        // Configura las acciones de input
        if (playerInput != null)
        {
            var actions = playerInput.actions;
            if (actions != null)
            {
                dashAction = actions["Dash"];
                moveAction = actions["Move"];
            }
        }
        // Guarda las escalas originales para efectos visuales
        if (visuals != null) originalVisualsScale = visuals.localScale;
        if (aimPivot != null) originalAimPivotScale = aimPivot.localScale;
    }

    private void Update()
    {
        // Solo permite dash si está disponible y la acción existe
        if (!canDash || dashAction == null) return;
        if (dashAction.WasPressedThisFrame())
        {
            // Obtiene la dirección inicial del dash
            Vector2 startDir = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            if (startDir.sqrMagnitude < 0.0001f)
                startDir = movementScript != null ? movementScript.GetLastMoveDirection() : Vector2.right;

            StartCoroutine(DashRoutine(startDir)); // Inicia la corrutina del dash
        }
    }

    // Corrutina que maneja la lógica del dash
    private System.Collections.IEnumerator DashRoutine(Vector2 startDirection)
    {
        canDash = false;
        isDashing = true;

        // Hace al jugador invencible durante el dash
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.SetTemporaryInvincibility(dashDuration);
        }

        Vector2 dashBaseDir = startDirection.normalized;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            // Permite ajustar la dirección del dash según el input
            Vector2 steer = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector2 finalDir = ((1f - steeringFactor) * dashBaseDir + steeringFactor * steer).normalized;

            // Aplica la velocidad al Rigidbody2D
            rb.linearVelocity = finalDir * dashSpeed;

            // Aplica efectos visuales de dash
            ApplyDashVisualEffects(finalDir);

            elapsed += Time.deltaTime;
            yield return null;
        }
        // Termina el dash
        rb.linearVelocity = Vector2.zero;
        ResetDashVisualEffects();
        isDashing = false;

        // Espera el cooldown antes de permitir otro dash
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Aplica efectos visuales durante el dash (estiramiento y rotación)
    private void ApplyDashVisualEffects(Vector2 dashDirection)
    {
        if (visuals == null) return;

        // Efecto de estiramiento y aplastamiento
        float stretchAmount = 1.5f;
        float squashAmount = 0.5f;

        Vector3 dashScale = new Vector3(
            originalVisualsScale.x * stretchAmount,
            originalVisualsScale.y * squashAmount,
            originalVisualsScale.z
        );

        visuals.localScale = dashScale;

        // Rota el aimPivot en la dirección del dash
        if (aimPivot != null)
        {
            float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Restaura los efectos visuales al terminar el dash
    private void ResetDashVisualEffects()
    {
        if (visuals != null)
            visuals.localScale = originalVisualsScale;
        // El aimPivot se corregirá automáticamente por PlayerAiming
    }

    // Permite interrumpir el dash manualmente
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