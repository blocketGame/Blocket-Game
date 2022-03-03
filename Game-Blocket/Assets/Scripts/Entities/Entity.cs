using UnityEngine;

[System.Serializable]
public class Entity{
    public string name;
    [Tooltip("Mob ID > 10000")]
    public uint entityId;
    public uint sizeX, sizeY;

}