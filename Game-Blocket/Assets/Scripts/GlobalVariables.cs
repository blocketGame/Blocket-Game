using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for Global (Unique) Variables/Settings
/// <br></br>
/// <b>Use it gently!</b>
/// </summary>
public static class GlobalVariables
{
	public static readonly ushort maxItemCountForMultiple = 128;
	public static GameVariables gameVariables = new GameVariables();

	#region Debug Variables
	public static readonly bool itemSlotButtonPressedLog = true;
	public static readonly bool itemTest = true;
	#endregion

	#region Setted Keys
	public static KeyCode openInventoryKey = KeyCode.E;
	public static KeyCode leftClick = KeyCode.Mouse0;
	public static KeyCode rightClick = KeyCode.Mouse1;
	#endregion

	/// <summary>
	/// Stores all Game Variables that are importent while running
	/// </summary>
	public class GameVariables {
		public bool gameRunning;

		public GameObject localPlayer, globalAssets, localUI;
		public readonly List<GameObject> players = new List<GameObject>();
		public bool isMultiplayer;
	}
}
