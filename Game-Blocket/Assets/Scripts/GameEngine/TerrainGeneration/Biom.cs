using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Biom : MonoBehaviour
{
    [SerializeField]
    private int index;
    [SerializeField]
    private int size;
    [SerializeField]
    private string biomName;
    [SerializeField]
    private OreData[] ores;
    [SerializeField]
    private RegionData[] regions;
    [SerializeField]
    private RegionData[] bgRegions;
    [SerializeField]
    private Decoration[] decorations;
    [SerializeField]
    private AnimationCurve heightCurve;
    [SerializeField]
    private Biomtype biomtype;

    //----------------------------------------------- Properties ----------------------------------------------------------------------------

    public int Index { get => index; set => index = value; }
    public int Size { get => size; set => size = value; }
    public string BiomName { get => biomName; set => biomName = value; }
    public Decoration[] Decorations { get => decorations; set => decorations = value; }
    public AnimationCurve HeightCurve { get => heightCurve; set => heightCurve = value; }
    public RegionData[] Regions { get => regions; set => regions = value; }
    public OreData[] Ores { get => ores; set => ores = value; }
    public RegionData[] BgRegions { get => bgRegions; set => bgRegions = value; }
    public Biomtype Biomtype { get => biomtype; set => biomtype = value; }
}

public enum Biomtype {
    
    OVERWORLD,UNDERGROUND,FLYING,SPACE,DEPTH
}
