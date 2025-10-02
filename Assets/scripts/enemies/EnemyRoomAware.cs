using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyRoomAware : MonoBehaviour
{
    private RoomController currentRoom; // guardar la sala

    void Start()
    {
        // Espera un poco para asegurarse de que todas las salas estén inicializadas antes de buscar
        Invoke(nameof(FindCurrentRoom), 0.1f);
    }
    
    void OnDestroy()
    {
        // notifica a la sala cuando este enemigo es destruido
        if (currentRoom != null)
        {
            currentRoom.UnregisterEnemy(gameObject);
        }
    }

    private void FindCurrentRoom()
    {
        // busca a todas las salas en la escena
        RoomController[] allRooms = FindObjectsOfType<RoomController>();
        Vector2 enemyPos = transform.position;
        // Recorre todas las salas y verifica si el enemigo está dentro de alguna
        foreach (RoomController room in allRooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            if (roomCollider != null && roomCollider.bounds.Contains(enemyPos))
            {
                currentRoom = room;
                currentRoom.RegisterEnemy(gameObject); // Registra el enemigo en la sala encontrada
                Debug.Log($"{name} assigned to room {room.name}");
                break;
            }
        }
        // Si no encuentra ninguna sala, muestra una advertencia
        if (currentRoom == null)
        {
            Debug.LogWarning($"{name} could not find a RoomController in the area!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si el enemigo entra en el trigger de otra sala
        if (other.CompareTag("Room"))
        {
            RoomController newRoom = other.GetComponent<RoomController>();
            if (newRoom != null && newRoom != currentRoom)
            {
                if (currentRoom != null)
                    currentRoom.UnregisterEnemy(gameObject);

                currentRoom = newRoom;
                currentRoom.RegisterEnemy(gameObject); // Registra el enemigo en la nueva sala
                Debug.Log($"{name} moved to room {newRoom.name}");
            }
        }
    }
}
