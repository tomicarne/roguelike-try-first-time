using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance; // Instancia singleton para acceso global
    public Transform spawnPoint; // Punto de aparición del jugador (asignar en el Inspector)

    // Se llama al crear el objeto, establece la instancia singleton
    void Awake()
    {
        instance = this;
    }

    // Método para reaparecer al jugador en el punto de spawn
    public void Respawn(GameObject player)
    {
        player.transform.position = spawnPoint.position; // Mueve al jugador al punto de aparición
        player.GetComponent<Health>().restore_health();  // Restaura la salud del jugador
    }
}