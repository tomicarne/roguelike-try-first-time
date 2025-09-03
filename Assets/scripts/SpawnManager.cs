using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance; 
    public Transform spawnPoint; // Assign in Inspector

    void Awake()
    {
        instance = this;
    }

    public void Respawn(GameObject player)
    {
        player.transform.position = spawnPoint.position;
        player.GetComponent<Health>().restore_health();
    }
}
