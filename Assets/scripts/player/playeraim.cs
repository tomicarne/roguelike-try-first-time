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

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCam = Camera.main;

        // Separate actions (defined in your .inputactions file)
        aimStick = playerInput.actions["AimStick"];   // <Gamepad>/rightStick
        aimMouse = playerInput.actions["AimMouse"];   // <Mouse>/position

        // Listen to any action to detect last device
    }

    private void OnDestroy()
    {
    }

    void Update()
    {
        // Check stick input first
        Vector2 stick = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        Vector2 mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;

        if (stick.sqrMagnitude > joystickDeadzone * joystickDeadzone)
            useMouse = false;
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
            useMouse = true;
        if (useMouse)
            AimWithMouse();
        else
            AimWithJoystick();
    }
    public void AimWithMouse()
    {
        Vector2 mouseScreenPos = aimMouse.ReadValue<Vector2>(); // screen pixels
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        Vector2 direction = (mouseWorldPos - transform.position);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    public void AimWithJoystick()
    {
        Vector2 aimInput = aimStick.ReadValue<Vector2>();

        if (aimInput.sqrMagnitude > joystickDeadzone * joystickDeadzone)
        {
            float angle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
}