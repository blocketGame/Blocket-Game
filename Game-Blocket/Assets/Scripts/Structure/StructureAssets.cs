using System.Collections.Generic;
using UnityEngine;

public class StructureAssets : MonoBehaviour{
	public List<Structure> Structures { get; } = new List<Structure>();

	private void Awake() { 
		GlobalVariables.StructureAssets = this;
		Structures.AddRange(GetComponentsInChildren<Structure>(true));
	}


	public void Start() => ReadAllStructures();

	public void ReadAllStructures(){
		foreach (Structure structure in Structures)
			structure.ReadStructureFromTilemap();
	}
}