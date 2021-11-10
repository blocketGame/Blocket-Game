using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for Global (Unique) Variables/Settings
/// <br></br>
/// <b>Use it gently!</b>
/// </summary>
public static class GlobalVariables{

	public static readonly ushort maxItemCountForMultiple = 128;
	public static LocalGameVariables localGameVariables = new LocalGameVariables();

	#region Debug Variables
	public static readonly bool itemSlotButtonPressedLog = true;
	public static readonly bool itemTest = true;
	#endregion

	#region Setted Keys
	public static KeyCode openInventoryKey = KeyCode.E;
	public static KeyCode leftClick = KeyCode.Mouse0;
	public static KeyCode rightClick = KeyCode.Mouse1;
	#endregion

	#region Properties for lazy people
	public static WorldData WorldData { get => localGameVariables.worldData; }
	public static TerrainGeneration TerrainGeneration { get => localGameVariables.terrainGeneration; }
	public static Inventory Inventory { get => localGameVariables.localPlayer.GetComponent<Inventory>(); }
	public static PlayerVariables PlayerVariables { get => localGameVariables.localPlayer.GetComponent<PlayerVariables>(); }
	public static Vector3 PlayerPos { get => localGameVariables.playerPos; }
	#endregion

	/// <summary>
	/// Stores all Game Variables that are importent while running
	/// </summary>
	public class LocalGameVariables {
		public bool gameRunning;

		public GameObject localPlayer, globalAssets, localUI, world;
		public readonly List<GameObject> players = new List<GameObject>();
		public bool isMultiplayer;

		public Vector3 playerPos;

		private GameObject _localPlayer;

		#region Vital Scripts

		public WorldData worldData;
		public TerrainGeneration terrainGeneration;

		#endregion
		

	}
}
