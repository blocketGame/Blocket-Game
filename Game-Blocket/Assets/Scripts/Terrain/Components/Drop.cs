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
	private uint _itemId;
	[SerializeField]
	private ushort _count;

	public Vector3 Position { get; set; }

	public uint ItemId { get => _itemId; set => _itemId = value; }
	public string Name { get => _name; set => _name = value; }
	public GameObject GameObject { get => _gameObject; set => _gameObject = value; }
	public ushort Count { get => _count; set => _count = value; }
	#endregion
}