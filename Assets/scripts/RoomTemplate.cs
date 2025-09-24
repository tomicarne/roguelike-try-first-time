using UnityEngine;

[CreateAssetMenu(fileName = "RoomTemplate", menuName = "Scriptable Objects/RoomTemplate")]
public class RoomTemplate : ScriptableObject
{
    public string name;
    public GameObject prefab;
    public bool doorNorth, doorSouth, doorEast, doorWest;
    public int difficulty;
}
