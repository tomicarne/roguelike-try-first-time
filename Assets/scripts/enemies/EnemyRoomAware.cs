using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyRoomAware : MonoBehaviour
{
    private RoomController currentRoom;

    void Start()
    {
        // Slight delay so all rooms are initialized before search
        Invoke(nameof(FindCurrentRoom), 0.1f);
    }

    void OnDestroy()
    {
        // Notify the room when this enemy is destroyed
        if (currentRoom != null)
        {
            currentRoom.UnregisterEnemy(gameObject);
        }
    }

    private void FindCurrentRoom()
    {
        RoomController[] allRooms = FindObjectsOfType<RoomController>();
        Vector2 enemyPos = transform.position;

        foreach (RoomController room in allRooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            if (roomCollider != null && roomCollider.bounds.Contains(enemyPos))
            {
                currentRoom = room;
                currentRoom.RegisterEnemy(gameObject);
                Debug.Log($"{name} assigned to room {room.name}");
                break;
            }
        }

        if (currentRoom == null)
        {
            Debug.LogWarning($"{name} could not find a RoomController in the area!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // If enemy enters a different room trigger
        if (other.CompareTag("Room"))
        {
            RoomController newRoom = other.GetComponent<RoomController>();
            if (newRoom != null && newRoom != currentRoom)
            {
                if (currentRoom != null)
                    currentRoom.UnregisterEnemy(gameObject);

                currentRoom = newRoom;
                currentRoom.RegisterEnemy(gameObject);
                Debug.Log($"{name} moved to room {newRoom.name}");
            }
        }
    }
}
