using UnityEngine;

public class RoomControllerTest : MonoBehaviour
{
    [Header("Door References (drag the actual child objects here)")]
    public GameObject northDoor;
    public GameObject southDoor;
    public GameObject eastDoor;
    public GameObject westDoor;

    [Header("Materials")]
    public Material openDoorMaterial;
    public Material closedDoorMaterial;

    void Start()
    {
        Debug.Log("=== RoomControllerTest Start ===");
        CheckDoor(northDoor, "North");
        CheckDoor(southDoor, "South");
        CheckDoor(eastDoor,  "East");
        CheckDoor(westDoor,  "West");

        // --- TEST 1: Set to CLOSED state ---
        Debug.Log("Setting all doors to CLOSED (solid, closedDoorMaterial)...");
        SetDoorState(northDoor, false);
        SetDoorState(southDoor, false);
        SetDoorState(eastDoor,  false);
        SetDoorState(westDoor,  false);

        // Wait 2 seconds then open again
        Invoke(nameof(OpenAllDoors), 2f);
    }

    void CheckDoor(GameObject door, string name)
    {
        if (door == null)
        {
            Debug.LogWarning($"{name} door is NULL!");
            return;
        }

        var sr = door.GetComponent<SpriteRenderer>();
        var col = door.GetComponent<Collider2D>();
        Debug.Log($"{name} door found. SpriteRenderer: {(sr!=null)}, Collider2D: {(col!=null)}");
    }

    void SetDoorState(GameObject door, bool isOpen)
    {
        if (door == null) return;

        var sr = door.GetComponent<SpriteRenderer>();
        var col = door.GetComponent<Collider2D>();

        if (sr != null)
        {
            sr.material = isOpen ? openDoorMaterial : closedDoorMaterial;
            sr.color = isOpen ? Color.green : Color.red;   // extra visual proof
        }
        if (col != null)
        {
            col.isTrigger = isOpen;                       // open = trigger
            Debug.Log($"{door.name} collider trigger now: {col.isTrigger}");
        }
    }

    void OpenAllDoors()
    {
        Debug.Log("Setting all doors to OPEN (trigger, openDoorMaterial)...");
        SetDoorState(northDoor, true);
        SetDoorState(southDoor, true);
        SetDoorState(eastDoor,  true);
        SetDoorState(westDoor,  true);
    }
}
