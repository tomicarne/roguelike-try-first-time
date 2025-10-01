using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public RoomType roomType = RoomType.Normal;
    [Header("Room Template")]
    public RoomTemplate template;

    [Header("Door References")]
    public GameObject northDoor;
    public GameObject southDoor;
    public GameObject eastDoor;
    public GameObject westDoor;

    [Header("Door Materials")]
    public Material openDoorMaterial;
    public Material closedDoorMaterial;

    public List<GameObject> enemiesInRoom = new();
    private bool playerInside = false;
    [Header("Spawning Setup")]
    private bool enemiesSpawned = false;
    public List<Transform> spawnPoints;      // Empty child objects inside the room
    public float spawnDelay = 0.3f;          // Optional delay between spawns
    private readonly List<GameObject> spawnedEnemies = new();

    void Start()
    {
        RoomController room = GetComponentInParent<RoomController>();
        if (room != null)
            room.RegisterEnemy(this.gameObject);
        // Rooms that require combat start with doors open (until player enters)
        SetDoorsVisualState(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        //ActivateAllEnemies(other.transform);
        if (!enemiesSpawned)
        {
            enemiesSpawned = true;
            StartCoroutine(SpawnEnemiesFromTemplate(other.transform));
        }
        // If this is a combat room and there are enemies, close doors
        CloseAllDoors();
        if (playerInside && enemiesInRoom.Count <= 1)
        {
            OpenAllDoors();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        enemiesInRoom.RemoveAll(e => e == null);
        if (playerInside && enemiesInRoom.Count <= 1)
        {
            OpenAllDoors();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    // ---- Enemy registration ----
    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemiesInRoom.Contains(enemy))
            enemiesInRoom.Add(enemy);
    }

    public void UnregisterEnemy(GameObject enemy)
    {

        enemiesInRoom.Remove(enemy);
        enemiesInRoom.RemoveAll(e => e == null);
        if (playerInside && enemiesInRoom.Count <= 1)
        {
            OpenAllDoors();
        }
        
    }

    // ---- Door control ----
    private void CloseAllDoors() => SetDoorsVisualState(false);
    private void OpenAllDoors() => SetDoorsVisualState(true);

    public void SetDoorsVisualState(bool open)
    {
        SetDoorState(northDoor, open);
        SetDoorState(southDoor, open);
        SetDoorState(eastDoor, open);
        SetDoorState(westDoor, open);
    }

    private void SetDoorState(GameObject door, bool isOpen)
    {
        if (door == null) return;

        // visual
        var sr = door.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (openDoorMaterial && closedDoorMaterial)
                sr.material = isOpen ? openDoorMaterial : closedDoorMaterial;
            sr.color = isOpen ? Color.green : Color.red;
        }

        // physics (open doors are triggers so the player can pass)
        var col = door.GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = isOpen;
    }

    // ---- Enemy activation ----
    /*private void ActivateAllEnemies(Transform player)
    {
        Debug.Log("activating enemies");
        foreach (var enemy in enemiesInRoom)
        {
            EnemyBase baseComp = enemy.GetComponent<EnemyBase>();
            Debug.Log($"{enemy.name}  EnemyBase? {baseComp != null}");
            if (baseComp != null)
                baseComp.Activate(player);
        }
    }*/
    private System.Collections.IEnumerator SpawnEnemiesFromTemplate(Transform player)
{
    
    if (template == null || template.enemyPrefabs == null || template.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{name}: No template or enemy prefabs set.");
            yield break;
        }

    // Decide how many enemies to spawn based on the template
    int enemyCount = Random.Range(template.minEnemies, template.maxEnemies + 1);

    for (int i = 0; i < enemyCount; i++)
    {
        // pick a random spawn point
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // choose a random enemy prefab from the template
        GameObject prefab = template.enemyPrefabs[Random.Range(0, template.enemyPrefabs.Length)];

        // instantiate and register
        GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
        var eh = enemy.GetComponent<EnemyHealth>();
        if (eh != null) eh.currentRoom = this;
        RegisterEnemy(enemy);
        spawnedEnemies.Add(enemy);

        // activate immediately if EnemyBase supports it
        EnemyBase baseComp = enemy.GetComponent<EnemyBase>();
        if (baseComp != null)
            baseComp.Activate(player);

        yield return new WaitForSeconds(spawnDelay);
    }
}

}
