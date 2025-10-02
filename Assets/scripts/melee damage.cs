using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public int damageAmount = 1; // Cantidad de daño que inflige la trampa

    // Se llama mientras un collider permanece dentro del trigger de la trampa
    private void OnTriggerStay2D(Collider2D collision)
    {
        // Verifica si el objeto que colisiona es el jugador
        if (collision.CompareTag("Player"))
            Debug.Log("Trap touching player");

        if (collision.CompareTag("Player"))
        {
            // Obtiene el componente PlayerHealth del jugador
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                // Aplica daño al jugador
                player.TakeDamage(damageAmount);
            }
        }
    }
}