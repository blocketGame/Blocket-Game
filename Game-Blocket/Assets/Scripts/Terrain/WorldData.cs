using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Class that is used to store running variables of an world
/// TODO: Move to TerrainGeneeration
/// <b>Author : Cse19455 / Thomas Boigner</b>
/// Hard modified by: HyFabi
/// </summary>
public class WorldData : MonoBehaviour {
	public static WorldData Singleton { get; protected set; }
	public GameObject ChunkParent => Grid.gameObject;

	public GameObject mobParent;
	public Grid Grid { get {
			
		return PlayerVariables.Dimension == Dimension.OVERWORLD ? GetComponentInChildren<Grid>() : GameObject.Find("Grid").GetComponentInChildren<Grid>();
		}
	}

	#region general fields
	[SerializeField]
	private int chunkDistance; //Settings
	[SerializeField]
	private int seed;
	[SerializeField]
	private int heightMultiplier;
	[SerializeField]
	private AnimationCurve heightcurve;
	[SerializeField]
	private int undergroundBiomStartChunk;
	[SerializeField]
	private int skyBiomStartChunk;
	[SerializeField]
	private float groupdistance;
	[SerializeField]
	private float pickUpDistance;

	#endregion

	#region overworld noise fields
	[SerializeField]
    private float scale;
	[SerializeField]
	private int octives;
	[SerializeField]
    [Range(0, 1)]
	private float persistance;
	[SerializeField]
	private float lacurinarity;
	[SerializeField]
	private float offsetX;
	[SerializeField]
	private float offsetY;
	#endregion

	#region underground noise fields
	[SerializeField]
	private float undergroundScale;
	[SerializeField]
	private int undergroundOctives;
	[SerializeField]
	[Range(0, 1)]
	private float undergroundPersistance;
	[SerializeField]
	private float undergroundLacurinarity;
	[SerializeField]
	private float undergroundOffsetX;
	[SerializeField]
	private float undergroundOffsetY;
	[SerializeField]
	private float initCaveSize;
	[SerializeField]
	private float stoneSize;
	[SerializeField]
	private float caveSizeIncrease;
	#endregion

	#region ore noise fields
	[SerializeField]
	private float oreScale;
	[SerializeField]
	private int oreOctives;
	[SerializeField]
	[Range(0, 1)]
	private float orePersistance;
	[SerializeField]
	private float oreLacurinarity;
	[SerializeField]
	private float oreOffsetX;
	[SerializeField]
	private float oreOffsetY;
	#endregion

	#region biom noise fields
	[SerializeField]
	private int biomOctives;
	[SerializeField]
	[Range(0, 1)]
	private float biomPersistance;
	[SerializeField]
	private float biomLacurinarity;
	[SerializeField]
	private float biomOffsetX;
	[SerializeField]
	private float biomOffsetY;
	#endregion

	#region sky noise fields
	[SerializeField]
	private float skyScale;
	[SerializeField]
	private int skyOctives;
	[SerializeField]
	[Range(0, 1)]
	private float skyPersistance;
	[SerializeField]
	private float skyLacurinarity;
	[SerializeField]
	private float skyOffsetX;
	[SerializeField]
	private float skyOffsetY;
	[SerializeField]
	private float ilandInitSize;
	[SerializeField]
	private float ilandSizeIncrease;
	[SerializeField]
	private float outsideSize;
	#endregion

	#region Properties
	//general Fields
	public int HeightMultiplier { get => heightMultiplier; set => heightMultiplier = value; }
	public AnimationCurve Heightcurve { get => heightcurve; set => heightcurve = value; }
	public int Seed { get => seed; set => seed = value; }
	public int ChunkDistance { get => chunkDistance; set => chunkDistance = value; }
	public int UndergroundBiomStartChunk { get => undergroundBiomStartChunk; set => undergroundBiomStartChunk = value; }
    public int SkyBiomStartChunk { get => skyBiomStartChunk; set => skyBiomStartChunk = value; }

	//Shortcuts
	[Obsolete]
	public int ChunkHeight => WorldAssets.ChunkLength;
	[Obsolete]
	public int ChunkWidth => WorldAssets.ChunkHeight;
	[Obsolete]
	public BlockData[] Blocks => WorldAssets.Singleton.blocks.ToArray();
	public float Groupdistance { get => groupdistance; set => groupdistance = value; }
	public float PickUpDistance { get => pickUpDistance; set => pickUpDistance = value; }
	public Dictionary<int, float[]> Noisemaps { get; set; } = new Dictionary<int, float[]>();

	//overworld noise fields 
	public float Persistance { get => persistance; set => persistance = value; }
	public float Lacurinarity { get => lacurinarity; set => lacurinarity = value; }
	public float OffsetX { get => offsetX; set => offsetX = value; }
	public float OffsetY { get => offsetY; set => offsetY = value; }
	public int Octives { get => octives; set => octives = value; }
	public float Scale { get => scale; set => scale = value; }


	//underground noise fields
	public float InitCaveSize { get => initCaveSize; set => initCaveSize = value; }
	public float StoneSize { get => stoneSize; set => stoneSize = value; }
	public float CaveSizeIncrease { get => caveSizeIncrease; set => caveSizeIncrease = value; }
    public float UndergroundScale { get => undergroundScale; set => undergroundScale = value; }
    public int UndergroundOctives { get => undergroundOctives; set => undergroundOctives = value; }
    public float UndergroundPersistance { get => undergroundPersistance; set => undergroundPersistance = value; }
    public float UndergroundLacurinarity { get => undergroundLacurinarity; set => undergroundLacurinarity = value; }
    public float UndergroundOffsetX { get => undergroundOffsetX; set => undergroundOffsetX = value; }
    public float UndergroundOffsetY { get => undergroundOffsetY; set => undergroundOffsetY = value; }

	//biom noise fields
    public int BiomOctives { get => biomOctives; set => biomOctives = value; }
    public float BiomPersistance { get => biomPersistance; set => biomPersistance = value; }
    public float BiomLacurinarity { get => biomLacurinarity; set => biomLacurinarity = value; }
    public float BiomOffsetX { get => biomOffsetX; set => biomOffsetX = value; }
    public float BiomOffsetY { get => biomOffsetY; set => biomOffsetY = value; }

	//sky noise fields
    public float SkyScale { get => skyScale; set => skyScale = value; }
    public int SkyOctives { get => skyOctives; set => skyOctives = value; }
    public float SkyPersistance { get => skyPersistance; set => skyPersistance = value; }
    public float SkyLacurinarity { get => skyLacurinarity; set => skyLacurinarity = value; }
    public float SkyOffsetX { get => skyOffsetX; set => skyOffsetX = value; }
    public float SkyOffsetY { get => skyOffsetY; set => skyOffsetY = value; }
    public float IlandInitSize { get => ilandInitSize; set => ilandInitSize = value; }
    public float OutsideSize { get => outsideSize; set => outsideSize = value; }
    public float IlandSizeIncrease { get => ilandSizeIncrease; set => ilandSizeIncrease = value; }

	//ore noise fields
    public float OreScale { get => oreScale; set => oreScale = value; }
    public int OreOctives { get => oreOctives; set => oreOctives = value; }
    public float OrePersistance { get => orePersistance; set => orePersistance = value; }
    public float OreLacurinarity { get => oreLacurinarity; set => oreLacurinarity = value; }
    public float OreOffsetX { get => oreOffsetX; set => oreOffsetX = value; }
    public float OreOffsetY { get => oreOffsetY; set => oreOffsetY = value; }


    #endregion

    /// <summary>Stores this class to <see cref="GlobalVariables"/></summary>
    public void Awake(){
		Singleton = this;
	}
}

public class OverworldData : WorldData{

}
public class DungeonworldData : WorldData{

}

#region WorldDataAssets

[System.Serializable]
public struct OreData
{
	[SerializeField]
	private string name;

	[SerializeField]
	private byte blockID;

	public string Name { get => name; set => name = value; }
	public byte BlockID { get => blockID; set => blockID = value; }
}

[System.Serializable]
public struct RegionData
{
	public string description;
	[SerializeField]
	private int regionRange; //-1 => infinite range
	[SerializeField]
	private byte blockID;

	public int RegionRange { get => regionRange; set => regionRange = value; }
	public byte BlockID { get => blockID; set => blockID = value; }
}


#endregion