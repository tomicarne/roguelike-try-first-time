using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    public float joystickDeadzone = 0.2f;

    private PlayerInput playerInput;
    private InputAction aimStick;
    private InputAction aimMouse;

    private Camera mainCam;
    private bool useMouse = true;

    // References to the new structure
    public Transform Visuals;
    public Transform aimPivot;
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // For animation blending
    private Vector2 lastAimDirection = Vector2.right;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCam = Camera.main;

        // Separate actions (defined in your .inputactions file)
        aimStick = playerInput.actions["AimStick"];   // <Gamepad>/rightStick
        aimMouse = playerInput.actions["AimMouse"];   // <Mouse>/position

        Transform visuals = transform.Find("Visuals");
        // Listen to any action to detect last device
    }

    void Update()
    {
        // Check stick input first
        HandleInputDeviceDetection();
        if (useMouse)
            AimWithMouse();
        else
            AimWithJoystick();

        UpdateAnimations();
    }

    void HandleInputDeviceDetection()
    {
        Vector2 stick = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        
        if (stick.sqrMagnitude > joystickDeadzone * joystickDeadzone)
            useMouse = false;
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
            useMouse = true;
    }

    public void AimWithMouse()
    {
        Vector2 mouseScreenPos = aimMouse.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        Vector2 direction = (mouseWorldPos - aimPivot.position).normalized;
        
        lastAimDirection = direction;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void AimWithJoystick()
    {
        Vector2 aimInput = aimStick.ReadValue<Vector2>();
        
        if (aimInput.sqrMagnitude > joystickDeadzone * joystickDeadzone)
        {
            lastAimDirection = aimInput.normalized;
            float angle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0, 0, angle);
        }

    }
    void UpdateAnimations()
    {
        Vector2 animDirection = lastAimDirection;

        animator.SetFloat("Horizontal", animDirection.x);
        animator.SetFloat("Vertical", animDirection.y);
    }
    
}