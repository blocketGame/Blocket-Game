using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 
/// </summary>
public class Decoration
{
    #region Properites + Regions
    [SerializeField]
    private byte _decorationID;
    [SerializeField]
    private TileBase _tile;
    [SerializeField]
    private string _name;
    [SerializeField]
    private Drop[] _drops;

    
    public byte DecorationID { get; set; }
    public TileBase Tile { get; set; }
    public string Name { get; set; }
    public Drop[] Drops { get; set; }
    #endregion
}

