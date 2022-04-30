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

	public static bool Multiplayer { get; set; } = false;

	[Obsolete]
	public static WorldData WorldData => WorldData.Singleton;

	public static GameObject ActivatedCraftingInterface { get; set; }
	public static GameObject LocalUI { get; set; }
	public static GameObject CraftingUIListContent { get; set; }
	public static GameObject World { get {
			if(_world != null)
				return _world;
			GameObject go = GameObject.Find(PlayerVariables.Dimension == Dimension.DUNGEON ? "Grid" : "World");
            World = go ?? throw new NullReferenceException();
			return go;
		}
		set => _world = value; }
	private static GameObject _world;
	public static Vector3 LocalPlayerPos { get => LocalPlayer.transform.position; }
	public static GameObject LocalPlayer { get; set; }

	public static void RemoveAllObjects(){
		if(LocalPlayer)
			UnityEngine.Object.Destroy(LocalPlayer);
		if(World)
			UnityEngine.Object.Destroy(World);
		if(LocalUI)
			UnityEngine.Object.Destroy(LocalUI);
	}

    /// <summary>Does the nothing</summary>
    public static void DoNothing(){
		//Nothing
		//DO NOT DELETE!
    }
}
