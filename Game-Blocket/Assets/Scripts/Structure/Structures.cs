using System.Collections.Generic;
using UnityEngine;

public class Structures : MonoBehaviour
{
    public List<Structure> structures = new List<Structure>();

    private void Awake() => GlobalVariables.Structures = this;

    public void ReadAllStructures()
    {
        foreach (Structure structure in structures)
        {
            structure.ReadStructureFromTilemap();
        }
    }
}