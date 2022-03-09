using System.Collections.Generic;
using UnityEngine;

public class StructureAssets : MonoBehaviour{
	public static PlayerVariables Singleton { get; private set; }

	public List<Structure> Structures = new List<Structure>();

	private void Awake() => Singleton = this;


	public void Start() => ReadAllStructures();

	public void ReadAllStructures(){
		foreach (Structure structure in Structures)
			structure.ReadStructureFromTilemap();
	}
}