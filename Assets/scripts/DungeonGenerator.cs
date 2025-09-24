using UnityEngine;
using System.Collections.Generic;   // <-- Capital “C” in Collections, required for List, Queue, Dictionary
using System;                        // for System.Random if you want to use it

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int roomCount = 12;
    public float roomSpacing = 20f;
    public RoomTemplate[] roomTemplates;
    public GameObject startRoom;
    public GameObject bossRoom;

    // Dictionary to keep track of placed rooms and their positions
    private Dictionary<Vector2Int, GameObject> placedRooms = new();   // <-- GameObject spelled correctly

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        Queue<Vector2Int> frontier = new();   // Vector2Int spelled correctly
        frontier.Enqueue(Vector2Int.zero);
        placedRooms[Vector2Int.zero] = Instantiate(startRoom, Vector3.zero, Quaternion.identity);

        int roomsPlaced = 1;
        System.Random rng = new System.Random();  // <-- System.Random, capital R

        while (frontier.Count > 0 && roomsPlaced < roomCount)
        {
            Vector2Int current = frontier.Dequeue();
            foreach (Vector2Int dir in Directions4())
            {
                Vector2Int next = current + dir;
                if (placedRooms.ContainsKey(next)) continue;  // <-- logic fixed: skip if ALREADY placed

                if (rng.NextDouble() < 0.5) continue; // 50% chance to branch

                RoomTemplate template = PickRoomTemplate(dir);   // fixed method name & type
                if (template == null) continue;

                Vector3 worldPos = new Vector3(next.x * roomSpacing, next.y * roomSpacing, 0);
                GameObject newRoom = Instantiate(template.prefab, worldPos, Quaternion.identity);
                placedRooms[next] = newRoom;
                frontier.Enqueue(next);
                roomsPlaced++;
                if (roomsPlaced >= roomCount) break;
            }
        }

        // Find farthest room for boss placement
        Vector2Int farthest = Vector2Int.zero;
        float maxDist = 0f;
        foreach (var kvp in placedRooms)
        {
            float d = kvp.Key.magnitude;   // Key, not key/hey
            if (d > maxDist) { maxDist = d; farthest = kvp.Key; }
        }

        Vector3 bossPos = new Vector3(farthest.x * roomSpacing, farthest.y * roomSpacing, 0);
        Instantiate(bossRoom, bossPos, Quaternion.identity);
    }

    // Four cardinal directions
    IEnumerable<Vector2Int> Directions4()
    {
        yield return Vector2Int.up;
        yield return Vector2Int.down;
        yield return Vector2Int.left;
        yield return Vector2Int.right;
    }

    RoomTemplate PickRoomTemplate(Vector2Int directionNeeded)
    {
        List<RoomTemplate> candidates = new();   // correct generic type name
        foreach (var t in roomTemplates)
        {
            if (directionNeeded == Vector2Int.up    && t.doorSouth) candidates.Add(t);
            if (directionNeeded == Vector2Int.down  && t.doorNorth) candidates.Add(t);
            if (directionNeeded == Vector2Int.left  && t.doorEast)  candidates.Add(t);
            if (directionNeeded == Vector2Int.right && t.doorWest)  candidates.Add(t);
        }
        if (candidates.Count == 0) return null;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
}
