using System.Collections;
using System.Collections.Generic;

using UnityEditor.SearchService;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used for Global (Unique) Variables/Settings
/// <br></br>
/// <b>Use it gently!</b>
/// 
/// </summary>
public static class GlobalVariables
{
	public static readonly Inventory inventory = new Inventory();

	#region Debug Variables
	public static bool itemSlotButtonPressedLog = false;
	#endregion
}
