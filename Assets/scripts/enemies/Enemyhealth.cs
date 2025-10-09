using UnityEngine;

public class EnemyHealth : Health
{
    [HideInInspector] public RoomController currentRoom;

    [Header("Item Drop Settings")]
    public GameObject[] itemDrop;      // Ítems posibles a soltar
    public bool dropAllItems = false;  // Si suelta todos o solo uno aleatorio
    [Range(0f, 1f)]
    public float dropChance = 0.5f;    // Probabilidad de soltar algo

    private bool hasDropped = false;

    protected override void Die()
    {
        Debug.Log(gameObject.name + " (Enemy) died!");
        DropItem();

        if (currentRoom != null)
            currentRoom.UnregisterEnemy(transform.root.gameObject);
        else
            Debug.LogWarning($"{name} died but had no RoomController reference.");

        Destroy(transform.root.gameObject);
    }

    private void DropItem()
    {
        if (hasDropped || itemDrop == null || itemDrop.Length == 0)
            return;

        // Verificar si el jugador ya tiene dañoExtra activo
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerSwordAttack sword = player.GetComponent<PlayerSwordAttack>();
            if (sword != null && sword.dañoExtra > 0)
            {
                Debug.Log($"{name} no soltó ítem porque el jugador ya tiene dañoExtra activo.");
                return;
            }
        }

        hasDropped = true;

        // Probabilidad de drop
        if (Random.value > dropChance)
        {
            Debug.Log($"{name} no soltó ítem esta vez.");
            return;
        }

        // Instancia el ítem
        if (dropAllItems)
        {
            foreach (GameObject itemPrefab in itemDrop)
            {
                GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.up, Quaternion.identity);
                AddPickupBehaviour(droppedItem);
            }
        }
        else
        {
            int index = Random.Range(0, itemDrop.Length);
            GameObject droppedItem = Instantiate(itemDrop[index], transform.position + Vector3.up, Quaternion.identity);
            AddPickupBehaviour(droppedItem);
        }

        Debug.Log($"{name} dropped item(s)!");
    }

    private void AddPickupBehaviour(GameObject item)
    {
        Collider2D col = item.GetComponent<Collider2D>();
        if (col == null)
        {
            col = item.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        ItemPickup pickup = item.AddComponent<ItemPickup>();
        pickup.playerTag = "Player";
    }
}

public class ItemPickup : MonoBehaviour
{
    public string playerTag = "Player";
    public float destroyDelay = 0f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"{name} recogido por el jugador.");

            // Aplicar el power-up de dañoExtra
            PlayerSwordAttack sword = other.GetComponent<PlayerSwordAttack>();
            if (sword != null)
            {
                sword.dañoExtra += 1;
                Debug.Log($"dañoExtra del jugador ahora es {sword.dañoExtra}");
            }

            // Destruir todos los demás ítems del mismo tipo en escena (usando la nueva API)
            ItemPickup[] allDrops = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);
            foreach (ItemPickup drop in allDrops)
            {
                if (drop != this)
                    Destroy(drop.gameObject);
            }

            // Destruir el ítem recogido
            Destroy(gameObject, destroyDelay);
        }
    }
}


