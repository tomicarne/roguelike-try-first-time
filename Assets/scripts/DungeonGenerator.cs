using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int minRooms = 8;
    public int maxRooms = 15;
    public float roomSpacing = 20f;
    public int maxBranchingDepth = 3;

    [Header("Room Templates")]
    public RoomTemplate[] roomTemplates;
    public GameObject startRoom;
    public GameObject bossRoom;
    public GameObject shopRoom;
    public GameObject treasureRoom;
    public GameObject secretRoom;

    [Header("Room Weights")]
    [Range(0, 100)] public int shopChance = 10;
    [Range(0, 100)] public int treasureChance = 15;
    [Range(0, 100)] public int secretRoomChance = 5;

    private readonly Dictionary<Vector2Int, RoomNode> roomGrid = new();
    private readonly List<RoomNode> endNodes = new();
    private System.Random rng;

    // Expose a read-only view of the grid so DoorPlacer (or others) can read it
    public IReadOnlyDictionary<Vector2Int, RoomNode> RoomGrid => roomGrid;

    [System.Serializable]
    public class RoomNode
    {
        public Vector2Int gridPosition;
        public GameObject roomInstance;
        public RoomType roomType = RoomType.Normal;
        public int depth;
        public RoomNode parent;
        public List<RoomNode> children = new();
        public bool isCriticalPath = false;

        public bool northDoor, southDoor, eastDoor, westDoor;
    }

    private void Start() => Generate();

    private void Generate()
    {
        rng = new System.Random();
        roomGrid.Clear();
        endNodes.Clear();

        // --- Start Room ---
        var startNode = new RoomNode
        {
            gridPosition = Vector2Int.zero,
            roomType = RoomType.Start,
            depth = 0
        };
        roomGrid[Vector2Int.zero] = startNode;
        startNode.roomInstance = Instantiate(startRoom, GetWorldPosition(Vector2Int.zero), Quaternion.identity);

        // --- Branching ---
        var toExpand = new Queue<RoomNode>();
        toExpand.Enqueue(startNode);

        int totalRooms = rng.Next(minRooms, maxRooms + 1);
        int roomsCreated = 1;

        while (toExpand.Count > 0 && roomsCreated < totalRooms)
        {
            RoomNode current = toExpand.Dequeue();
            if (current.depth >= maxBranchingDepth) continue;

            int branchCount = GetBranchCount(current.depth);
            var availableDirs = GetAvailableDirections(current.gridPosition)
                                .OrderBy(_ => rng.Next()).ToList();

            for (int i = 0; i < Mathf.Min(branchCount, availableDirs.Count) && roomsCreated < totalRooms; i++)
            {
                Vector2Int dir = availableDirs[i];
                Vector2Int newPos = current.gridPosition + dir;
                if (roomGrid.ContainsKey(newPos)) continue;

                RoomType newType = DetermineRoomType(current.depth, roomsCreated, totalRooms);
                RoomNode newNode = CreateRoomNode(newPos, dir, current, newType);

                roomGrid[newPos] = newNode;
                current.children.Add(newNode);
                toExpand.Enqueue(newNode);
                roomsCreated++;

                // Potential end node
                if (branchCount == 1 || i == branchCount - 1)
                    endNodes.Add(newNode);
            }
        }

        IdentifyCriticalPath();
        PlaceSpecialRooms();

        // Recompute final doors from the grid adjacency (must happen AFTER layout)
        RecomputeDoors();

        InstantiateAllRooms();

        // Optionally: if you have a DoorPlacer on the same GameObject, call it now
        var placer = GetComponent<DoorPlacer>();
        if (placer != null) placer.PlaceDoors();
    }

    private int GetBranchCount(int depth)
    {
        float[] probs = { 0.6f, 0.3f, 0.1f };
        float roll = (float)rng.NextDouble();
        float sum = 0;
        for (int i = 0; i < probs.Length; i++)
        {
            sum += probs[i];
            if (roll <= sum) return i + 1;
        }
        return 1;
    }

    private List<Vector2Int> GetAvailableDirections(Vector2Int pos)
    {
        var dirs = new List<Vector2Int>();
        foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            if (!roomGrid.ContainsKey(pos + d)) dirs.Add(d);
        return dirs;
    }

    private RoomType DetermineRoomType(int depth, int made, int total)
    {
        if (depth < 2) return RoomType.Normal;
        int roll = rng.Next(0, 100);

        if (roll < secretRoomChance) return RoomType.Secret;
        if (roll < secretRoomChance + treasureChance) return RoomType.Treasure;
        if (roll < secretRoomChance + treasureChance + shopChance) return RoomType.Shop;

        if (made > total * 0.7f && rng.Next(0, 100) < 30)
            return RoomType.Elite;

        return RoomType.Normal;
    }

    private RoomNode CreateRoomNode(Vector2Int pos, Vector2Int dir, RoomNode parent, RoomType type)
    {
        var node = new RoomNode
        {
            gridPosition = pos,
            roomType = type,
            depth = parent.depth + 1,
            parent = parent
        };

        if (dir == Vector2Int.up) { parent.northDoor = true; node.southDoor = true; }
        if (dir == Vector2Int.down) { parent.southDoor = true; node.northDoor = true; }
        if (dir == Vector2Int.right) { parent.eastDoor = true; node.westDoor = true; }
        if (dir == Vector2Int.left) { parent.westDoor = true; node.eastDoor = true; }

        return node;
    }

    private void IdentifyCriticalPath()
    {
        RoomNode farthest = null;
        int maxDepth = 0;
        foreach (var node in roomGrid.Values)
            if (node.depth > maxDepth) { maxDepth = node.depth; farthest = node; }

        RoomNode bossNode = farthest;
        while (farthest != null)
        {
            farthest.isCriticalPath = true;
            farthest = farthest.parent;
        }

        if (bossNode != null) bossNode.roomType = RoomType.Boss;
    }

    private void RecomputeDoors()
    {
        foreach (var kvp in roomGrid)
        {
            var pos = kvp.Key;
            var n = kvp.Value;
            n.northDoor = roomGrid.ContainsKey(pos + Vector2Int.up);
            n.southDoor = roomGrid.ContainsKey(pos + Vector2Int.down);
            n.eastDoor = roomGrid.ContainsKey(pos + Vector2Int.right);
            n.westDoor = roomGrid.ContainsKey(pos + Vector2Int.left);
        }
    }

    private void PlaceSpecialRooms()
    {
        var shopCandidates = roomGrid.Values
            .Where(n => n.isCriticalPath && n.depth > 2 && n.depth < maxBranchingDepth - 1 && n.roomType == RoomType.Normal)
            .ToList();
        if (shopCandidates.Count > 0)
            shopCandidates[rng.Next(shopCandidates.Count)].roomType = RoomType.Shop;

        var deadEnds = roomGrid.Values.Where(n => n.children.Count == 0 && n.roomType == RoomType.Normal);
        foreach (var end in deadEnds)
            if (rng.Next(0, 100) < 40) end.roomType = RoomType.Treasure;
    }

    private void InstantiateAllRooms()
    {
        Debug.Log("=== INSTANTIATING ROOMS ===");

        foreach (var node in roomGrid.Values)
        {
            if (node.roomInstance != null)
            {
                Debug.Log($"Room at {node.gridPosition} already instantiated (start room).");
                continue;
            }

            // choose template & rotation first
            RoomTemplate template = roomTemplates.FirstOrDefault(t =>
                t.doorNorth == node.northDoor &&
                t.doorSouth == node.southDoor &&
                t.doorEast == node.eastDoor &&
                t.doorWest == node.westDoor);

            GameObject prefabToUse;
            Quaternion rotation = Quaternion.identity;

            if (template != null)
            {
                prefabToUse = template.prefab;
                rotation = Quaternion.Euler(template.defaultRotation);
            }
            else
            {
                prefabToUse = GetRoomPrefab(node);
            }

            if (prefabToUse == null) continue;

            node.roomInstance = Instantiate(prefabToUse,
                                            GetWorldPosition(node.gridPosition),
                                            rotation);

            Debug.Log($"✓ Instantiated {node.roomType} at {node.gridPosition} using '{prefabToUse.name}'");

            var rc = node.roomInstance.GetComponent<RoomController>();
            if (rc != null)
            {
                rc.roomType = node.roomType;
                if (template != null) rc.template = template;
                rc.SetDoorsVisualState(true);
            }
        }

        Debug.Log($"Total instantiated rooms: {roomGrid.Values.Count}");
    }

    private GameObject GetRoomPrefab(RoomNode node)
    {
        switch (node.roomType)
        {
            case RoomType.Boss: Debug.Log($"  BOSS ROOM: Using bossRoom prefab '{bossRoom?.name}'"); return bossRoom;
            case RoomType.Shop: Debug.Log($"  SHOP ROOM: Using shopRoom prefab '{shopRoom?.name}'"); return shopRoom;
            case RoomType.Treasure: Debug.Log($"  TREASURE ROOM: Using treasureRoom prefab '{treasureRoom?.name}'"); return treasureRoom;
            case RoomType.Secret: Debug.Log($"  SECRET ROOM: Using secretRoom prefab '{secretRoom?.name}'"); return secretRoom;
            case RoomType.Start: Debug.Log($"  START ROOM: Using startRoom prefab '{startRoom?.name}'"); return startRoom;
            default:
                var matches = roomTemplates.Where(t =>
                    t.doorNorth == node.northDoor &&
                    t.doorSouth == node.southDoor &&
                    t.doorEast == node.eastDoor &&
                    t.doorWest == node.westDoor).ToArray();
                return matches.Length > 0
                    ? matches[rng.Next(matches.Length)].prefab
                    : (roomTemplates.Length > 0 ? roomTemplates[0].prefab : null);
        }
    }

    // renamed helper (was GridToWorld) -> public so other scripts can call it without ambiguity
    public Vector3 GetWorldPosition(Vector2Int grid) =>
        new Vector3(grid.x * roomSpacing, grid.y * roomSpacing, 0);

    private void OnDrawGizmos()
    {
        if (roomGrid == null) return;
        foreach (var kvp in roomGrid)
        {
            Vector3 pos = GetWorldPosition(kvp.Key);
            var node = kvp.Value;

            Gizmos.color = node.roomType switch
            {
                RoomType.Start => Color.green,
                RoomType.Boss => Color.red,
                RoomType.Shop => Color.yellow,
                RoomType.Treasure => Color.magenta,
                RoomType.Secret => Color.cyan,
                RoomType.Elite => Color.white,
                _ => node.isCriticalPath ? Color.blue : Color.gray
            };
            Gizmos.DrawCube(pos, Vector3.one * 3f);

            Gizmos.color = Color.white;
            foreach (var child in node.children)
                Gizmos.DrawLine(pos, GetWorldPosition(child.gridPosition));

            Gizmos.color = Color.black;   // arrow colour
            float a = 2f;                 // arrow length
            float h = 0.5f;               // half arrow-head size

            if (node.northDoor)
                DrawArrow(pos, Vector3.up * a, h);
            if (node.southDoor)
                DrawArrow(pos, Vector3.down * a, h);
            if (node.eastDoor)
                DrawArrow(pos, Vector3.right * a, h);
            if (node.westDoor)
                DrawArrow(pos, Vector3.left * a, h);
        }
    }

    private void DrawArrow(Vector3 start, Vector3 dir, float headSize)
    {
        Vector3 end = start + dir;
        Gizmos.DrawLine(start, end);

        Vector3 right = Quaternion.LookRotation(Vector3.forward) * Quaternion.AngleAxis(150, Vector3.forward) * dir.normalized * headSize;
        Vector3 left = Quaternion.LookRotation(Vector3.forward) * Quaternion.AngleAxis(-150, Vector3.forward) * dir.normalized * headSize;
        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
    }
}
