using UnityEngine;

public class DamageTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    public int damageAmount = 1;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        Debug.Log("Trap touching player");
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }
        }
    }
}
