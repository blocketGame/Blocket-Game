using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemySO : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    private int spawnchance;
    [SerializeField]
    private GameObject enemiePrefab;
    [SerializeField]
    private Biomtype spawnhabitat;

    public int Spawnchance { get => spawnchance; set => spawnchance = value; }
    public GameObject EnemiePrefab { get => enemiePrefab; set => enemiePrefab = value; }
    public Biomtype Spawnhabitat { get => spawnhabitat; set => spawnhabitat = value; }

    public void OnAfterDeserialize()
    {
        //[Todo]
    }

    public void OnBeforeSerialize()
    {
        //[Todo]
    }
}