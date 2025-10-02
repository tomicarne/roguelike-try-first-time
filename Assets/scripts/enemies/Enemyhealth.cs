using UnityEngine;

public class EnemyHealth : Health
{
    [HideInInspector] public RoomController currentRoom;
    // la funcion cuando la vida del enemy baja a 0
    protected override void Die()
    {
        Debug.Log(gameObject.name + " (Enemy) died!");
        //se quita del registro de la sala
        if (currentRoom != null)
            currentRoom.UnregisterEnemy(transform.root.gameObject);
        else
            Debug.LogWarning($"{name} died but had no RoomController reference.");
        Destroy(transform.root.gameObject);
    }   
}
