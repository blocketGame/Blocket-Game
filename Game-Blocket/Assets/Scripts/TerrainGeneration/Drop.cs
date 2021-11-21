using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO Move to <see cref="Item"/>
/// </summary>
public class Drop{
	#region Fields + Properties
	[SerializeField]
	private GameObject _gameObject;
	[SerializeField]
	private string _name;
	[SerializeField]
	private byte _dropID, _count;

	public byte DropID { get => _dropID; set => _dropID = value; }
	public string Name { get => _name; set => _name = value; }
	public GameObject GameObject { get => _gameObject; set => _gameObject = value; }
	public byte Count { get => _count; set => _count = value; }
	#endregion
}