using UnityEngine;

public class EnemyHealth : Health
{
    [HideInInspector] public RoomController currentRoom;
    protected override void Die()
    {
        Debug.Log(gameObject.name + " (Enemy) died!");
        if (currentRoom != null)
            currentRoom.UnregisterEnemy(gameObject);
        else
            Debug.LogWarning($"{name} died but had no RoomController reference.");
        Destroy(transform.root.gameObject);
    }   
}
