using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop
{
    [SerializeField]
    private GameObject _dropObject;
    [SerializeField]
    private string _dropName;
    [SerializeField]
    private byte _dropID;

    public byte DropID { get => _dropID; set => _dropID = value; }
    public string DropName { get => _dropName; set => _dropName = value; }
    public GameObject DropObject { get => _dropObject; set => _dropObject = value; }
}

public class Item
{
    [SerializeField]
    private GameObject _itemObject;
    [SerializeField]
    private string _itemname;
    [SerializeField]
    private byte _itemID;
    [SerializeField]
    private Drop[] _drops;

    public Drop[] Drops { get => _drops; set => _drops = value; }
    public byte ItemID { get => _itemID; set => _itemID = value; }
    public string Itemname { get => _itemname; set => _itemname = value; }
    public GameObject ItemObject { get => _itemObject; set => _itemObject = value; }
}
