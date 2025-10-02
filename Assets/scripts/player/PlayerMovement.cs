using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f; // Velocidad de movimiento del jugador

    private Rigidbody2D rb; // Referencia al Rigidbody2D del jugador
    private Vector2 moveInput; // Almacena la entrada de movimiento actual
    private Vector2 lastMoveDirection = Vector2.right; // Última dirección de movimiento

    private PlayerInput playerInput; // Referencia al componente PlayerInput
    private InputAction moverAction; // Acción de input para moverse
    private PlayerDash dash; // Referencia al script de dash


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Desactiva la gravedad para un juego 2D top-down

        playerInput = GetComponent<PlayerInput>();
        moverAction = playerInput.actions["Move"]; // Obtiene la acción de movimiento

        dash = GetComponent<PlayerDash>(); // Obtiene el componente de dash si existe
    }


    void Update()
    {
        // No actualiza el movimiento si está haciendo dash
        if (dash != null && dash.IsDashing) return;

        moveInput = moverAction.ReadValue<Vector2>(); // Lee la entrada de movimiento
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput.normalized; // Actualiza la última dirección si hay movimiento
        }
    }


    void FixedUpdate()
    {
        // No mueve al jugador si está haciendo dash
        if (dash != null && dash.IsDashing) return;
        rb.linearVelocity = moveInput * moveSpeed; // Aplica la velocidad al Rigidbody2D
    }

    // Devuelve la última dirección de movimiento
    public Vector2 GetLastMoveDirection() { return lastMoveDirection; }
}