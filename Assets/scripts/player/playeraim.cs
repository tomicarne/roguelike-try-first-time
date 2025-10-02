using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAiming : MonoBehaviour
{
    [Header("Aiming Settings")]
    public float joystickDeadzone = 0.2f; // Zona muerta para el stick derecho

    [Header("Camera Look-Ahead")]
    public bool enableLookAhead = true; // Si la cámara debe mirar hacia donde apuntas
    public float maxLookAheadDistance = 3f; // Distancia máxima de anticipación de cámara
    public float lookAheadSmoothness = 2f;  // Suavidad del movimiento de cámara

    [Header("Input Multipliers")]
    public float mouseLookMultiplier = 1.5f;      // Multiplicador para el mouse
    public float controllerLookMultiplier = 1f;   // Multiplicador para el control

    private PlayerInput playerInput;      // Referencia al componente PlayerInput
    private InputAction aimStick;         // Acción para el stick derecho
    private InputAction aimMouse;         // Acción para el mouse

    private Camera mainCam;               // Referencia a la cámara principal
    private bool useMouse = true;         // Si se está usando mouse para apuntar

    // Referencias a objetos visuales y animaciones
    public Transform Visuals;             // Referencia al objeto visual del jugador
    public Transform aimPivot;            // Punto de pivote para rotar el arma o sprite
    public Animator animator;             // Referencia al Animator
    private SpriteRenderer spriteRenderer;// Referencia al SpriteRenderer

    // Para blending de animaciones
    private Vector2 lastAimDirection = Vector2.right; // Última dirección de apuntado
    private Vector3 cameraLookTarget;                 // Objetivo de la cámara

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mainCam = Camera.main;

        // Obtiene las acciones de input definidas en el InputActions
        aimStick = playerInput.actions["AimStick"];   // <Gamepad>/rightStick
        aimMouse = playerInput.actions["AimMouse"];   // <Mouse>/position

        Transform visuals = transform.Find("Visuals");
        // Aquí podrías inicializar spriteRenderer si lo necesitas
    }

    void Update()
    {
        // Detecta si se está usando mouse o stick
        HandleInputDeviceDetection();
        if (useMouse)
            AimWithMouse();
        else
            AimWithJoystick();

        UpdateAnimations();
        UpdateCameraLookAhead();
    }

    // Detecta el último dispositivo de entrada usado para apuntar
    void HandleInputDeviceDetection()
    {
        Vector2 stick = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;

        if (stick.sqrMagnitude > joystickDeadzone * joystickDeadzone)
            useMouse = false;
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.1f)
            useMouse = true;
    }

    // Apunta usando el mouse
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

    // Apunta usando el stick derecho del control
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
            // Si no se apunta, la cámara regresa al jugador
            cameraLookTarget = transform.position;
        }
    }

    // Actualiza los parámetros del Animator según la dirección de apuntado
    void UpdateAnimations()
    {
        Vector2 animDirection = lastAimDirection;

        animator.SetFloat("Horizontal", animDirection.x);
        animator.SetFloat("Vertical", animDirection.y);
    }

    // Permite activar o desactivar el look-ahead de la cámara
    public void SetLookAheadEnabled(bool enabled)
    {
        enableLookAhead = enabled;
        if (!enabled && mainCam != null)
        {
            // Regresa la cámara al jugador inmediatamente
            Vector3 playerPos = transform.position;
            mainCam.transform.position = new Vector3(playerPos.x, playerPos.y, mainCam.transform.position.z);
        }
    }

    // Mueve suavemente la cámara hacia el objetivo de look-ahead
    void UpdateCameraLookAhead()
    {
        if (!enableLookAhead || mainCam == null) return;
        
        Vector3 cameraPos = mainCam.transform.position;
        Vector3 targetPos = new Vector3(cameraLookTarget.x, cameraLookTarget.y, cameraPos.z);
        
        mainCam.transform.position = Vector3.Lerp(cameraPos, targetPos, lookAheadSmoothness * Time.deltaTime);
    }
    
    // Permite cambiar la distancia máxima de look-ahead desde otros scripts
    public void SetLookAheadDistance(float distance)
    {
        maxLookAheadDistance = distance;
    }
}