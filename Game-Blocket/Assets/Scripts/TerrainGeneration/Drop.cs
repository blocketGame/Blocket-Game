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
	[SerializeField]
	private byte anzahl;

	#region Properties
	public byte DropID { get => _dropID; set => _dropID = value; }
	public string DropName { get => _dropName; set => _dropName = value; }
	public GameObject DropObject { get => _dropObject; set => _dropObject = value; }
	public byte Anzahl { get => anzahl; set => anzahl = value; }
	#endregion
}