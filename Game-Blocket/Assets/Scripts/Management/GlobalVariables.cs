using System;

using UnityEngine;

/// <summary>
/// Used for Global (Unique) Variables/Settings
/// <br></br>
/// <b>Use it gently!</b>
/// </summary>
public static class GlobalVariables {

	public static readonly ushort maxItemCountForMultiple = 128;
	public static string chunkTag = "Chunk";

	[Obsolete]
	public static WorldData WorldData => WorldData.Singleton;

	public static GameObject ActivatedCraftingInterface { get; set; }
	public static GameObject LocalUI { get; set; }
	public static GameObject CraftingUIListContent { get; set; }
	public static GameObject World { get; set; }
	public static Vector3 LocalPlayerPos { get => LocalPlayer.transform.position; }
	public static GameObject LocalPlayer { get; set; }

    /// <summary>Does the nothing</summary>
    public static void DoNothing(){
		//Nothing
		//DO NOT DELETE!
    }
}
