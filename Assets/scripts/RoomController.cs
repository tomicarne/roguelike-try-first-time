using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public RoomType roomType = RoomType.Normal; // Tipo de sala (normal, jefe, etc.)
    [Header("Room Template")]
    public RoomTemplate template; // Plantilla que define qué enemigos pueden aparecer

    [Header("Door References")]
    public GameObject northDoor;
    public GameObject southDoor;
    public GameObject eastDoor;
    public GameObject westDoor;

    [Header("Door Materials")]
    public Material openDoorMaterial;   // Material para puertas abiertas
    public Material closedDoorMaterial; // Material para puertas cerradas

    public List<GameObject> enemiesInRoom = new(); // Lista de enemigos presentes en la sala
    private bool playerInside = false;             // Si el jugador está dentro de la sala

    [Header("Spawning Setup")]
    private bool enemiesSpawned = false;           // Si los enemigos ya fueron generados
    public List<Transform> spawnPoints;            // Puntos de aparición de enemigos
    public float spawnDelay = 0.3f;                // Retardo opcional entre spawns
    private readonly List<GameObject> spawnedEnemies = new(); // Lista de enemigos generados

    // Se llama al iniciar la sala
    void Start()
    {
        RoomController room = GetComponentInParent<RoomController>();
        if (room != null)
            room.RegisterEnemy(this.gameObject);
        // Las salas de combate empiezan con puertas abiertas hasta que entra el jugador
        SetDoorsVisualState(true);
    }

    // Se llama cuando el jugador entra en el trigger de la sala
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        // Genera enemigos si aún no han sido generados
        if (!enemiesSpawned)
        {
            enemiesSpawned = true;
            StartCoroutine(SpawnEnemiesFromTemplate(other.transform));
        }
        // Si es una sala de combate y hay enemigos, cierra las puertas
        CloseAllDoors();
        // Si ya no quedan enemigos, abre las puertas
        if (playerInside && enemiesInRoom.Count <= 1)
        {
            OpenAllDoors();
        }
    }

    // Se llama mientras el jugador permanece en la sala
    void OnTriggerStay2D(Collider2D collision)
    {
        enemiesInRoom.RemoveAll(e => e == null); // Limpia enemigos destruidos
        if (playerInside && enemiesInRoom.Count <= 1)
        {
            OpenAllDoors();
        }
    }

    // Se llama cuando el jugador sale de la sala
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    // Agrega un enemigo a la lista de la sala
    public void RegisterEnemy(GameObject enemy)
    {
        if (!enemiesInRoom.Contains(enemy))
            enemiesInRoom.Add(enemy);
    }

    // Remueve un enemigo de la lista de la sala
    public void UnregisterEnemy(GameObject enemy)
    {
        enemiesInRoom.Remove(enemy);
        enemiesInRoom.RemoveAll(e => e == null); // Limpia referencias nulas
        if (playerInside && enemiesInRoom.Count <= 1)
        {
            OpenAllDoors();
        }
    }

    // ---- Control de puertas ----
    private void CloseAllDoors() => SetDoorsVisualState(false); // Cierra todas las puertas
    private void OpenAllDoors() => SetDoorsVisualState(true);   // Abre todas las puertas

    // Cambia el estado visual y físico de todas las puertas
    public void SetDoorsVisualState(bool open)
    {
        SetDoorState(northDoor, open);
        SetDoorState(southDoor, open);
        SetDoorState(eastDoor, open);
        SetDoorState(westDoor, open);
    }

    // Cambia el estado de una puerta individual
    private void SetDoorState(GameObject door, bool isOpen)
    {
        if (door == null) return;

        // Cambia el material y color de la puerta
        var sr = door.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (openDoorMaterial && closedDoorMaterial)
                sr.material = isOpen ? openDoorMaterial : closedDoorMaterial;
            sr.color = isOpen ? Color.green : Color.red;
        }

        // Cambia el collider para permitir o bloquear el paso
        var col = door.GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = isOpen;
    }

    // ---- Generación y activación de enemigos ----
    // Corrutina que genera enemigos según la plantilla de la sala
    private System.Collections.IEnumerator SpawnEnemiesFromTemplate(Transform player)
    {
        if (template == null || template.enemyPrefabs == null || template.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{name}: No template or enemy prefabs set.");
            yield break;
        }

        // Decide cuántos enemigos generar según la plantilla
        int enemyCount = Random.Range(template.minEnemies, template.maxEnemies + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            // Elige un punto de aparición aleatorio
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];

            // Elige un prefab de enemigo aleatorio de la plantilla
            GameObject prefab = template.enemyPrefabs[Random.Range(0, template.enemyPrefabs.Length)];

            // Instancia y registra el enemigo
            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            var eh = enemy.GetComponent<EnemyHealth>();
            if (eh != null) eh.currentRoom = this;
            RegisterEnemy(enemy);
            spawnedEnemies.Add(enemy);

            // Activa el enemigo si tiene el componente EnemyBase
            EnemyBase baseComp = enemy.GetComponent<EnemyBase>();
            if (baseComp != null)
                baseComp.Activate(player);

            yield return new WaitForSeconds(spawnDelay);
        }
    }
}