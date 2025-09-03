using UnityEngine;

public class PlayerHealth : Health
{
    public GameObject deathScreen; // assign in Inspector if you want UI
    public bool respawnOnDeath = true;

    protected override void Die()
    {
        Debug.Log("Player died!");

        if (deathScreen != null)
            deathScreen.SetActive(true);

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