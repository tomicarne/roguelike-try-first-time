using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f; // Movement speed

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;

    private PlayerInput playerInput;
    private InputAction moverAction;
    private PlayerDash dash;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        playerInput = GetComponent<PlayerInput>();
        moverAction = playerInput.actions["Move"];

        dash = GetComponent<PlayerDash>();
    }

    void Update()
    {
        // dont update if we are dashing
        if (dash != null && dash.IsDashing) return;

        moveInput = moverAction.ReadValue<Vector2>();
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    void FixedUpdate()
    {
        if (dash != null && dash.IsDashing) return;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public Vector2 GetLastMoveDirection() { return lastMoveDirection; }
}