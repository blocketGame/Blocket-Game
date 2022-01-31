using UnityEngine;

/// <summary>
/// Used for Global (Unique) Variables/Settings
/// <br></br>
/// <b>Use it gently!</b>
/// </summary>
public static class GlobalVariables {

	public static readonly ushort maxItemCountForMultiple = 128;
	public static string chunkTag = "Chunk";

	public static GameManager GameManager { get; set; }
	
	#region UIScripts
	public static UILobby UILobby { get; set; }
	public static UIProfileSite UIProfileSite { get; set; }
	public static UIInventory UIInventory { get; set; }
	public static PlayerHealth PlayerHealth { get; set; }
	public static GameObject LocalUI {
		get => _localUI; set {
			_localUI = value;
			UIInventory = value.GetComponentInChildren<UIInventory>();
		}
	}
	private static GameObject _localUI;
	#endregion

	#region World Scripts
	
	public static WorldData WorldData { get; set; }
	public static TerrainGeneration TerrainGeneration { get; set; }
	public static TerrainHandler TerrainHandler { get; set;}
	public static GameObject World { get => _world; set { 
			_world = value;
		} 
	}
	private static GameObject _world;
	#endregion

	#region Assets
	public static PrefabAssets PrefabAssets { get; set; }
	public static ItemAssets ItemAssets { get; set; }
	public static WorldAssets WorldAssets { get; set; }
	#endregion

	#region LocalPlayer
	public static Inventory Inventory { get => _inventory; }
	private static Inventory _inventory; 
	public static PlayerVariables PlayerVariables { get => _playerVariables; }
	private static PlayerVariables _playerVariables;
	public static Vector3 LocalPlayerPos { get => LocalPlayer.transform.position; }

	public static Movement Movement { get; set; }

	public static GameObject LocalPlayer
	{
		get => _localPlayer; set
		{
			_localPlayer = value;
			_playerVariables = value.GetComponent<PlayerVariables>();
			_inventory = value.GetComponentInChildren<Inventory>();
		}
	}
    private static GameObject _localPlayer;

	#endregion
}
