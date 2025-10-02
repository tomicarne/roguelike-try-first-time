using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    // minimo y maximo de salas
    public int minRooms = 8;
    public int maxRooms = 15;
    // distancia entre salas
    public float roomSpacing = 20f;
    // maxima cantidad de salas en cada branch
    public int maxBranchingDepth = 3;

    [Header("Room Templates")]
    // los prefabs de salas normales
    public RoomTemplate[] roomTemplates;
    //salas especiales
    public GameObject startRoom;
    public GameObject bossRoom;
    public GameObject shopRoom;
    public GameObject treasureRoom;
    public GameObject secretRoom;

    [Header("Room Weights")]
    // probabilidad de salas especiales
    [Range(0, 100)] public int shopChance = 10;
    [Range(0, 100)] public int treasureChance = 15;
    [Range(0, 100)] public int secretRoomChance = 5;

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
        // puertas de las cuatro direcciones
        public bool northDoor, southDoor, eastDoor, westDoor;
    }
    // para que cuando se empieze la partida se genera la dungeon
    private void Start() => Generate();

    private void Generate()
    {
        rng = new System.Random();
        // limpiar por si hay nodes del anterior intento
        roomGrid.Clear();
        endNodes.Clear();

        // Crear el nodo inicial (sala de inicio) en la posición (0,0)
        var startNode = new RoomNode
        {
            gridPosition = Vector2Int.zero,
            roomType = RoomType.Start,
            depth = 0
        };
        // Registrar la sala de inicio en el grid
        roomGrid[Vector2Int.zero] = startNode;
        // Instanciar el prefab de la sala de inicio en la escena
        startNode.roomInstance = Instantiate(startRoom, GetWorldPosition(Vector2Int.zero), Quaternion.identity);
        // Cola para expandir nodos (BFS)
        var toExpand = new Queue<RoomNode>();
        toExpand.Enqueue(startNode);

        // Determinar la cantidad total de salas a crear
        int totalRooms = rng.Next(minRooms, maxRooms + 1);
        int roomsCreated = 1;

        // Expandir la dungeon mientras haya nodos por expandir y no se alcance el límite de salas
        while (toExpand.Count > 0 && roomsCreated < totalRooms)
        {
            RoomNode current = toExpand.Dequeue();
            // Limitar la profundidad de ramificación
            if (current.depth >= maxBranchingDepth) continue;

            // Determinar cuántas ramas crear desde este nodo
            int branchCount = GetBranchCount(current.depth);
            // Obtener direcciones disponibles y mezclarlas aleatoriamente
            var availableDirs = GetAvailableDirections(current.gridPosition)
                    .OrderBy(_ => rng.Next()).ToList();

            // Crear nuevas salas en las direcciones disponibles
            for (int i = 0; i < Mathf.Min(branchCount, availableDirs.Count) && roomsCreated < totalRooms; i++)
            {
            Vector2Int dir = availableDirs[i];
            Vector2Int newPos = current.gridPosition + dir;
            // Saltar si ya existe una sala en esa posición
            if (roomGrid.ContainsKey(newPos)) continue;

            // Determinar el tipo de sala a crear
            RoomType newType = DetermineRoomType(current.depth, roomsCreated, totalRooms);
            // Crear el nodo de la nueva sala
            RoomNode newNode = CreateRoomNode(newPos, dir, current, newType);

            // Registrar la nueva sala en el grid y como hija del nodo actual
            roomGrid[newPos] = newNode;
            current.children.Add(newNode);
            // Agregar la nueva sala a la cola para expandirla después
            toExpand.Enqueue(newNode);
            roomsCreated++;

            // Si es un nodo final potencial (poca ramificación), agregarlo a endNodes
            if (branchCount == 1 || i == branchCount - 1)
                endNodes.Add(newNode);
            }
        }

        IdentifyCriticalPath();
        PlaceSpecialRooms();
        RecomputeDoors();
        InstantiateAllRooms();
    }
    // identifica cuantas branch hay
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
    // identifica las dirrecciones disponibles para colocar una sala
    private List<Vector2Int> GetAvailableDirections(Vector2Int pos)
    {
        var dirs = new List<Vector2Int>();
        foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            if (!roomGrid.ContainsKey(pos + d)) dirs.Add(d);
        return dirs;
    }
    // determina el tipo de sala que sera seleccionada
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
    // crea un nodo para una sala
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
    // identifica el camino critico para la conexion entre la sala de incicio y de boss
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
    // calcula las puertas necesarias
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
    // coloca las sala especiales
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
    // crea las salas 
    private void InstantiateAllRooms()
    {
        Debug.Log("=== INSTANTIATING ROOMS ===");

        foreach (var node in roomGrid.Values)
        {
            // verifica si la sala ya existe en el nodo deseado
            if (node.roomInstance != null)
            {
                Debug.Log($"Room at {node.gridPosition} already instantiated (start room).");
                continue;
            }

            // escoge el prefab y la rotacion necesaria
            RoomTemplate template = roomTemplates.FirstOrDefault(t =>
                t.doorNorth == node.northDoor &&
                t.doorSouth == node.southDoor &&
                t.doorEast == node.eastDoor &&
                t.doorWest == node.westDoor);

            GameObject prefabToUse;
            Quaternion rotation = Quaternion.identity;
            // sacara la rotacion necesaria para el prefab
            if (template != null)
            {
                prefabToUse = template.prefab;
                rotation = Quaternion.Euler(template.defaultRotation);
            }
            else //escogera el primer prefab en la lista
            {
                prefabToUse = GetRoomPrefab(node);
            }

            if (prefabToUse == null) continue; // verifica que si se haya seleccionado un prefab

            // coloca el prefab en ka scena
            node.roomInstance = Instantiate(prefabToUse,
                                            GetWorldPosition(node.gridPosition),
                                            rotation);

            Debug.Log($"✓ Instantiated {node.roomType} at {node.gridPosition} using '{prefabToUse.name}'");

            // guarda el room controller del prefab
            var rc = node.roomInstance.GetComponent<RoomController>();
            if (rc != null)
            {
                rc.roomType = node.roomType;
                if (template != null) rc.template = template;
                // abre todas las puertas
                rc.SetDoorsVisualState(true);
            }
        }

        Debug.Log($"Total instantiated rooms: {roomGrid.Values.Count}");
    }

    // para poder seleccionar los prefabs en las listas
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
    // selecciona la posicion de un nodo
    public Vector3 GetWorldPosition(Vector2Int grid) =>
        new Vector3(grid.x * roomSpacing, grid.y * roomSpacing, 0);

    // guia para saber como esta funcionando el algoritmo
    private void OnDrawGizmos()
    {
        if (roomGrid == null) return;
        foreach (var kvp in roomGrid)
        {
            Vector3 pos = GetWorldPosition(kvp.Key);
            var node = kvp.Value;
            //selecciona el color de la conexion dependiendo del tipo de sala
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
            // estableze el color de la flecha
            Gizmos.color = Color.black;   // color de la flecha
            float a = 4f;                 // distancia de la flecha
            float h = 1f;               // tamaño de la cabeza 
            // dibuja una flecha dependiendo en donde se nececite la conexion para las salas
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
