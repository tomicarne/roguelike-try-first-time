using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    [Header("Aiming Settings")]
    public float joystickDeadzone = 0.2f;

    [Header("Camera Look-Ahead")]
    public bool enableLookAhead = true;
    public float maxLookAheadDistance = 3f;
    public float lookAheadSmoothness = 2f;

    [Header("Input Multipliers")]
    public float mouseLookMultiplier = 1.5f;
    public float controllerLookMultiplier = 1f;

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
    private Vector3 cameraLookTarget;

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
        UpdateCameraLookAhead();
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
        if (enableLookAhead)
        {
            Vector2 lookDirection = (mouseWorldPos - transform.position).normalized;
            float distance = Vector2.Distance(mouseWorldPos, transform.position);
            float lookDistance = Mathf.Min(distance * 0.3f, maxLookAheadDistance) * mouseLookMultiplier;
            cameraLookTarget = transform.position + (Vector3)lookDirection * lookDistance;
        }
    }
    public void AimWithJoystick()
    {
        Vector2 aimInput = aimStick.ReadValue<Vector2>();

        if (aimInput.sqrMagnitude > joystickDeadzone * joystickDeadzone)
        {
            lastAimDirection = aimInput.normalized;
            float angle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            aimPivot.rotation = Quaternion.Euler(0, 0, angle);
            if (enableLookAhead)
            {
                float lookDistance = aimInput.magnitude * maxLookAheadDistance * controllerLookMultiplier;
                cameraLookTarget = transform.position + (Vector3)aimInput.normalized * lookDistance;
            }
        }
        else if (enableLookAhead)
        {
            // Return camera to player when not aiming
            cameraLookTarget = transform.position;
        }


    }
    void UpdateAnimations()
    {
        Vector2 animDirection = lastAimDirection;

        animator.SetFloat("Horizontal", animDirection.x);
        animator.SetFloat("Vertical", animDirection.y);
    }
        public void SetLookAheadEnabled(bool enabled)
    {
        enableLookAhead = enabled;
        if (!enabled && mainCam != null)
        {
            // Snap camera back to player
            Vector3 playerPos = transform.position;
            mainCam.transform.position = new Vector3(playerPos.x, playerPos.y, mainCam.transform.position.z);
        }
    }
    void UpdateCameraLookAhead()
    {
        if (!enableLookAhead || mainCam == null) return;
        
        // Smoothly move camera towards look target
        Vector3 cameraPos = mainCam.transform.position;
        Vector3 targetPos = new Vector3(cameraLookTarget.x, cameraLookTarget.y, cameraPos.z);
        
        mainCam.transform.position = Vector3.Lerp(cameraPos, targetPos, lookAheadSmoothness * Time.deltaTime);
    }
    
    public void SetLookAheadDistance(float distance)
    {
        maxLookAheadDistance = distance;
    }
    
}