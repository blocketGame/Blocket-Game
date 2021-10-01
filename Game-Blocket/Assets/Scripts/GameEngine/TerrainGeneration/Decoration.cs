using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Decoration
{
    [SerializeField]
    private byte _decorationID;
    [SerializeField]
    private TileBase _tile;
    [SerializeField]
    private string _name;
    [SerializeField]
    private Drop[] _drops;

    byte DecorationID { get; set; }
    TileBase Tile { get; set; }
    string Name { get; set; }
    Drop[] Drops { get; set; }

}

