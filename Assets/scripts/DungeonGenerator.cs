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
    [Range(0,100)] public int shopChance = 10;
    [Range(0,100)] public int treasureChance = 15;
    [Range(0,100)] public int secretRoomChance = 5;

    private readonly Dictionary<Vector2Int, RoomNode> roomGrid = new();
    private readonly List<RoomNode> endNodes = new();
    private System.Random rng;

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
        startNode.roomInstance = Instantiate(startRoom, GridToWorld(Vector2Int.zero), Quaternion.identity);

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
        SyncDoors();
        InstantiateAllRooms();
    }

    private int GetBranchCount(int depth)
    {
        float[] probs = {0.6f, 0.3f, 0.1f};
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
        foreach (var d in new[]{Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right})
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

        if (dir == Vector2Int.up)    { parent.northDoor = true; node.southDoor = true; }
        if (dir == Vector2Int.down)  { parent.southDoor = true; node.northDoor = true; }
        if (dir == Vector2Int.right) { parent.eastDoor  = true; node.westDoor  = true; }
        if (dir == Vector2Int.left)  { parent.westDoor  = true; node.eastDoor  = true; }

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

        if (farthest != null) farthest.roomType = RoomType.Boss;
    }
    private void SyncDoors()
{
    foreach (var kvp in roomGrid)
    {
        Vector2Int pos = kvp.Key;
        RoomNode node = kvp.Value;

        if (roomGrid.TryGetValue(pos + Vector2Int.up, out var north))
        {
            node.northDoor = true;
            north.southDoor = true;
        }
        if (roomGrid.TryGetValue(pos + Vector2Int.down, out var south))
        {
            node.southDoor = true;
            south.northDoor = true;
        }
        if (roomGrid.TryGetValue(pos + Vector2Int.left, out var west))
        {
            node.westDoor = true;
            west.eastDoor = true;
        }
        if (roomGrid.TryGetValue(pos + Vector2Int.right, out var east))
        {
            node.eastDoor = true;
            east.westDoor = true;
        }
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

            if (node.roomInstance != null) { Debug.Log($"  Room at {node.gridPosition} already instantiated (Start room)"); continue; }
            var prefab = GetRoomPrefab(node);
            if (prefab == null) continue;

            node.roomInstance = Instantiate(prefab,
                                            GridToWorld(node.gridPosition),
                                            Quaternion.identity);

            // --- NEW: hook up doors & template ---
            var rc = node.roomInstance.GetComponent<RoomController>();
            Debug.Log($"  ✓ Instantiated {node.roomType} room at {node.gridPosition} with prefab '{prefab.name}'");
            if (rc != null)
            {
                rc.roomType = node.roomType;

                // find a RoomTemplate whose door config matches this node
                var template = roomTemplates.FirstOrDefault(t =>
                    t.doorNorth == node.northDoor &&
                    t.doorSouth == node.southDoor &&
                    t.doorEast == node.eastDoor &&
                    t.doorWest == node.westDoor);

                if (template != null)
                {
                    node.roomInstance = Instantiate(
                        template.prefab,
                        GridToWorld(node.gridPosition),
                        Quaternion.Euler(template.defaultRotation)
                    );
                rc.template = template;
                Debug.Log($"    Assigned RoomTemplate '{template.roomName}' to RoomController");

                // doors start visually open
                rc.SetDoorsVisualState(true);
                Debug.Log($"    Connected RoomController, doors set open");
            }
            else
            {
                Debug.LogWarning($"    No RoomController found on {node.roomType} prefab '{prefab.name}' at {node.gridPosition}");
            }
        }
        Debug.Log($"Total instantiated rooms: {roomGrid.Values.Count}");
    }
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

    private Vector3 GridToWorld(Vector2Int grid) =>
        new Vector3(grid.x * roomSpacing, grid.y * roomSpacing, 0);

    private void OnDrawGizmos()
    {
        if (roomGrid == null) return;
        foreach (var kvp in roomGrid)
        {
            Vector3 pos = GridToWorld(kvp.Key);
            var node = kvp.Value;

            Gizmos.color = node.roomType switch
            {
                RoomType.Start    => Color.green,
                RoomType.Boss     => Color.red,
                RoomType.Shop     => Color.yellow,
                RoomType.Treasure => Color.magenta,
                RoomType.Secret   => Color.cyan,
                RoomType.Elite    => Color.white,
                _ => node.isCriticalPath ? Color.blue : Color.gray
            };
            Gizmos.DrawCube(pos, Vector3.one * 3f);

            Gizmos.color = Color.white;
            foreach (var child in node.children)
                Gizmos.DrawLine(pos, GridToWorld(child.gridPosition));
        }
    }
}