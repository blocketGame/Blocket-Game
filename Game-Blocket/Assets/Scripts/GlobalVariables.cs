using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for Global (Unique) Variables/Settings
/// <br></br>
/// <b>Use it gently!</b>
/// </summary>
public static class GlobalVariables {

	public static readonly ushort maxItemCountForMultiple = 128;

	#region Debug Variables
	public static readonly bool itemSlotButtonPressedLog = true;
	public static readonly bool itemTest = true;
	#endregion

	#region Setted Keys
	public static KeyCode openInventoryKey = KeyCode.E;
	public static KeyCode leftClick = KeyCode.Mouse0;
	public static KeyCode rightClick = KeyCode.Mouse1;
	#endregion

	#region Properties + Fields
	public static GameManager GameManager { get; set; }
	public static GameObject localUI;
	//public static readonly List<GameObject> players = new List<GameObject>();

	#region Assets
	public static GameObject GlobalAssets { get => _globalAssets; set { 
			_globalAssets = value;
			_prefabAssets = value.GetComponent<PrefabAssets>();
		}
	}
	private static GameObject _globalAssets;
	public static PrefabAssets PrefabAssets { get => _prefabAssets; }
	private static PrefabAssets _prefabAssets;
	#endregion

	public static WorldData WorldData { get; set; }
	public static TerrainGeneration TerrainGeneration { get; set; }

	public static GameObject World { get => _world; set { 
			_world = value;
		} 
	}
	private static GameObject _world;

	#region LocalPlayer
	public static Inventory Inventory { get => _inventory; }
	private static Inventory _inventory;
	public static PlayerVariables PlayerVariables { get => _playerVariables; }
	private static PlayerVariables _playerVariables;
	public static Vector3 PlayerPos { get => _playerPos; }
	private static Vector3 _playerPos;

	public static GameObject LocalPlayer { get => _localPlayer; set {
			_localPlayer = value;
			_playerVariables = value.GetComponent<PlayerVariables>();
			_inventory = value.GetComponentInChildren<Inventory>();
			_playerPos = value.transform.position;
		}
	}	
	private static GameObject _localPlayer;
	#endregion
	#endregion
}
