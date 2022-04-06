using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Biom : ScriptableObject, ISerializationCallbackReceiver {
	#region Defaultsettings
	[SerializeField]
	private int index;
	[SerializeField]
	private string biomName;
	#endregion

	#region BiomDataStoring
	[SerializeField]
	private OreData[] ores;
	[SerializeField]
	private RegionData[] regions;
	[SerializeField]
	private RegionData[] bgRegions;
	[SerializeField]
	private Decoration[] decorations;
	[SerializeField]
	private byte[] structures;
	[SerializeField]
	private byte[] enemies;
	[SerializeField]
	private byte stoneBlockId;
	[SerializeField]
	private byte stoneBlockIdBg;
	[SerializeField]
	private byte skyBlockInsideId;
	[SerializeField]
	private byte skyBlockInsideIdBg;
	[SerializeField]
	private byte skyBlockOutsideId;
	[SerializeField]
	private byte skyBlockOutsideBg;
	#endregion

	#region TerrainAndSpawn
	[SerializeField]
	private int size;
	[SerializeField]
	private AnimationCurve heightCurve;
	[SerializeField]
	private List<Biomtype> biomtype;
	#endregion

	#region Props
	public int Index { get => index; set => index = value; }
	public int Size { get => size; set => size = value; }
	public string BiomName { get => biomName; set => biomName = value; }
	public Decoration[] Decorations { get => decorations; set => decorations = value; }
	public AnimationCurve HeightCurve { get => heightCurve; set => heightCurve = value; }
	public RegionData[] Regions { get => regions; set => regions = value; }
	public OreData[] Ores { get => ores; set => ores = value; }
	public RegionData[] BgRegions { get => bgRegions; set => bgRegions = value; }
	public List<Biomtype> Biomtype { get => biomtype; set => biomtype = value; }
    public byte[] Structures { get => structures; set => structures = value; }
	public byte StoneBlockId { get => stoneBlockId; set => stoneBlockId = value; }
    public byte StoneBlockIdBg { get => stoneBlockIdBg; set => stoneBlockIdBg = value; }
    public byte SkyBlockInsideId { get => skyBlockInsideId; set => skyBlockInsideId = value; }
    public byte SkyBlockInsideIdBg { get => skyBlockInsideIdBg; set => skyBlockInsideIdBg = value; }
    public byte SkyBlockOutsideId { get => skyBlockOutsideId; set => skyBlockOutsideId = value; }
    public byte SkyBlockOutsideBg { get => skyBlockOutsideBg; set => skyBlockOutsideBg = value; }
    #endregion

    public void OnAfterDeserialize() {
		//[TODO]
		
	}

	public void OnBeforeSerialize() {
		//[TODO]
	}
}

/// <summary>
/// Used to determine where it should spawn
/// </summary>
public enum Biomtype {
	OVERWORLD,
	UNDERGROUND,
	FLYING,
	SPACE,
	DEPTH
}

