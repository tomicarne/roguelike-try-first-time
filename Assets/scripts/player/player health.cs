using UnityEngine;

public class PlayerHealth : Health
{
<<<<<<< Updated upstream
    public GameObject deathScreen; // assign in Inspector if you want UI
    public bool respawnOnDeath = true;

=======
    public GameObject deathScreen; // Pantalla de muerte (UI)
    public bool respawnOnDeath = true; // Si el jugador debe reaparecer al morir

    private PlayerSwordAttack swordAttack; // Referencia al script del ataque

    protected override void Start()
    {
        // Busca el componente PlayerSwordAttack en el mismo objeto
        swordAttack = GetComponent<PlayerSwordAttack>();
    }

    // Se llama cuando la salud llega a 0
>>>>>>> Stashed changes
    protected override void Die()
    {
        Debug.Log("Player died!");

        if (deathScreen != null)
            deathScreen.SetActive(true);

        //Reinicia los power-ups al morir
        if (swordAttack != null)
        {
            swordAttack.canThrowSword = false;
            swordAttack.canReflectBullets = false;
            swordAttack.dañoExtra = 0;
            Debug.Log("Power-ups reiniciados al morir.");
        }

        if (respawnOnDeath)
        {
            // Example respawn
            transform.position = Vector3.zero;
            currentHealth = maxHealth;
            Debug.Log("Player respawned!");
        }
        else
        {
            // If no respawn, just disable player
            gameObject.SetActive(false);
        }
    }
}
