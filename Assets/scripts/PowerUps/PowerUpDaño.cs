using UnityEngine;

public class PowerUpDaño : MonoBehaviour
{
    public float dañoExtra;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerSwordAttack playerSwordAttack))
        {
            playerSwordAttack.SubirDañoPowerUp(dañoExtra);
            //Destroy(gameObject); // Destruye el power-up después de recogerlo
        }
    }
}
