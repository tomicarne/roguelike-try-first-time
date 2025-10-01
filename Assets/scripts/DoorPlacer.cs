using UnityEngine;

// Places a single door prefab between adjacent room cells.
// Attach this to the same GameObject as DungeonGenerator (or assign generator in Inspector).
[ExecuteAlways]
public class DoorPlacer : MonoBehaviour
{
    public DungeonGenerator generator;   // assign in Inspector (or left null => will try GetComponent)
    public GameObject doorPrefab;        // standalone door prefab (sprite + collider)
    public Transform doorParent;         // optional parent for placed doors
    [Tooltip("Fraction of roomSpacing used for offset; usually 0.5")]
    public float doorOffsetFactor = 0.5f;

    // Call this after the generator finished creating rooms
    public void PlaceDoors()
    {
        if (generator == null) generator = GetComponent<DungeonGenerator>();
        if (generator == null || doorPrefab == null) return;

        // delete previous doors created by this script first
        ClearPlacedDoors();

        var grid = generator.RoomGrid;
        float spacing = generator.roomSpacing;
        float half = spacing * doorOffsetFactor;

        foreach (var kv in grid)
        {
            Vector2Int pos = kv.Key;
            var node = kv.Value;

            // Put doors only for Right and Up neighbors to avoid duplicates.
            // Every adjacent pair will be processed exactly once.
            if (node.eastDoor && grid.ContainsKey(pos + Vector2Int.right))
                PlaceSingleDoor(pos, Vector2Int.right, half);

            if (node.northDoor && grid.ContainsKey(pos + Vector2Int.up))
                PlaceSingleDoor(pos, Vector2Int.up, half);
        }
    }

    private void PlaceSingleDoor(Vector2Int origin, Vector2Int dir, float offset)
    {
        Vector2Int neighbour = origin + dir;

        Vector3 originWorld = generator.GetWorldPosition(origin);
        Vector3 doorPos = originWorld + (Vector3)((Vector2)dir * offset);

        Quaternion rot = Quaternion.identity;
        // rotate for East/West connections so door sprite faces the correct way
        if (dir == Vector2Int.right || dir == Vector2Int.left)
            rot = Quaternion.Euler(0, 0, 90f); // tweak this depending on your sprite

        Transform parent = doorParent != null ? doorParent : transform;
        var go = Instantiate(doorPrefab, doorPos, rot, parent);
        go.name = $"Door_{origin.x}_{origin.y}_{dir.x}_{dir.y}";
    }

    // removes doors created by this script (child GameObjects named "Door_...")
    private void ClearPlacedDoors()
    {
        Transform parent = doorParent != null ? doorParent : transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform c = parent.GetChild(i);
            if (c.name.StartsWith("Door_"))
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) DestroyImmediate(c.gameObject); else
#endif
                    Destroy(c.gameObject);
            }
        }
    }
}
