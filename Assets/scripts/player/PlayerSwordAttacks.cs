using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerSwordAttack : MonoBehaviour
{
    [Header("References")]
    public GameObject swordHitbox;           // Referencia al hitbox de la espada
    private Collider2D swordCollider;        // Referencia al collider del hitbox
    public Animator animator;                // Referencia al Animator del sprite del jugador
    public Transform aimPivot;               // Referencia al pivote de apuntado
    public BoomerangSword boomerang;         // Referencia al script de la espada boomerang

    [Header("Attack Settings")]
    public float attackDuration = 0.3f;      // Duración del ataque cuerpo a cuerpo
    public float attackDistance = 1f;        // Distancia a la que aparece el hitbox de la espada

    [Header("Upgrades")]
    public bool canReflectBullets = false;   // Si el jugador puede reflejar balas
    public bool canThrowSword = false;       // Si el jugador puede lanzar la espada

    private bool attacking = false;          // Si el jugador está atacando actualmente
    private PlayerInput playerInput;         // Referencia al componente PlayerInput
    private InputAction attackAction;        // Acción de input para atacar
    private SpriteRenderer spriteRenderer;   // Referencia al SpriteRenderer (no usado aquí)
    [HideInInspector] public bool isThrowing = false; // Si la espada está lanzada
    public float tiempoPowerUp;
    public float dañoExtra;
    public float dañoBaseGolpe = 1f; // Daño base del golpe de espada  

    // Inicializa referencias y desactiva el hitbox al iniciar
    void Start()
    {
        swordHitbox.SetActive(false);
        swordCollider = swordHitbox.GetComponent<Collider2D>();
        swordCollider.enabled = false;
        
        playerInput = GetComponent<PlayerInput>();

        // Obtiene la acción de ataque del Input Actions
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

    // Se llama cada frame
    void Update()
    {
        // No permite atacar si ya está atacando o la espada está lanzada
        if (attacking || isThrowing) return;

        // Ataque con mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(SwingSword());
        }
        // Ataque con control (botón asignado en Input Actions)
        else if (attackAction != null && attackAction.WasPressedThisFrame())
        {
            StartCoroutine(SwingSword());
        }
        // Ataque con teclado (tecla K)
        else if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            StartCoroutine(SwingSword());
        }

        // Lanzar espada si tiene la mejora y presiona espacio
        if (canThrowSword && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("trhown");
            Vector2 throwDir = aimPivot.right.normalized;
            boomerang.gameObject.SetActive(true);
            boomerang.Throw(transform, this, throwDir);
        }
    }

    // Corrutina para el ataque cuerpo a cuerpo
    private System.Collections.IEnumerator SwingSword()
    {
        attacking = true;
        swordCollider.enabled = true;
        swordHitbox.SetActive(true);
        // Posiciona el hitbox frente al jugador
        PositionSwordHitbox();

        // Mantiene el hitbox activo durante la duración del ataque
        yield return new WaitForSeconds(attackDuration);
        swordHitbox.SetActive(false);
        swordCollider.enabled = false;
        attacking = false;
    }

    // Posiciona el hitbox de la espada frente al jugador según el pivote de apuntado
    private void PositionSwordHitbox()
    {
        if (aimPivot == null) return;
        
        Vector2 attackDirection = aimPivot.right;
        swordHitbox.transform.position = aimPivot.position + (Vector3)(attackDirection * attackDistance);

        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        swordHitbox.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Devuelve la dirección de apuntado actual
    private Vector2 GetAimDirection()
    {
        return aimPivot.right;
    }

    // Detecta colisiones del hitbox de la espada
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!attacking) return;

        // Daña enemigos si los golpea
        Health target = other.GetComponent<Health>();
        if (target != null && other.CompareTag("Enemy"))
        {
            target.TakeDamage(1);
        }

        // Refleja balas si tiene la mejora, si no las destruye
        if (other.CompareTag("EnemyBullet"))
        {
            if (canReflectBullets)
            {
                Rigidbody2D brb = other.GetComponent<Rigidbody2D>();
                if (brb != null)
                {
                    Vector2 aimDir = GetAimDirection();
                    other.tag = "playerBullet";

                    // Redirige la bala reflejada
                    EnemyBullet eb = other.GetComponent<EnemyBullet>();
                    if (eb != null) eb.Redirect(aimDir);
                }
            }
            else
            {
                // Destruye la bala si no puede reflejar
                Destroy(other.gameObject);
            }
        }
    }
}
