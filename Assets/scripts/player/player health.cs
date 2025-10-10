using UnityEngine;

// Maneja la salud y muerte del jugador, hereda de Health
public class PlayerHealth : Health
{
    public GameObject deathScreen; // Pantalla de muerte (UI)
    public bool respawnOnDeath = true; // Si el jugador debe reaparecer al morir

    // Se llama cuando la salud llega a 0
    protected override void Die()
    {
        Debug.Log("Player died!");

        // Muestra la pantalla de muerte si está asignada
        if (deathScreen != null)
            deathScreen.SetActive(true);

        if (respawnOnDeath)
        {
            // Ejemplo de respawn: mueve al jugador al origen y restaura la salud
            transform.position = Vector3.zero;
            currentHealth = maxHealth;
            Debug.Log("Player respawned!");
        }
        else
        {
            // Si no hay respawn, desactiva el GameObject del jugador
            gameObject.SetActive(false);
        }
    }
}