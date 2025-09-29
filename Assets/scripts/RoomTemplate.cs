using UnityEngine;

[CreateAssetMenu(fileName = "RoomTemplate", menuName = "Scriptable Objects/RoomTemplate")]
public class RoomTemplate : ScriptableObject
{
    [Header("Basic Info")]
    public string roomName;
    public GameObject prefab;
    
    [Header("Door Configuration")]
    public bool doorNorth;
    public bool doorSouth;
    public bool doorEast;
    public bool doorWest;

    [Tooltip("Default rotation so the doors face the declared directions")]
    public Vector3 defaultRotation = Vector3.zero;
    
    [Header("Room Properties")]
    public int difficulty = 1;
    public RoomType roomType = RoomType.Normal;
    
    [Header("Spawn Weights")]
    public float commonWeight = 1f;
    public float rareWeight = 0.1f;
    
    [Header("Enemy Spawns")]
    public int minEnemies = 3;
    public int maxEnemies = 8;
    public GameObject[] enemyPrefabs;
}