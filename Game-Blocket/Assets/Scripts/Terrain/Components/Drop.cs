using System;

using UnityEngine;

using Random = System.Random;

/// <summary>
/// TODO Move to <see cref="Item"/>
/// </summary>
public class Drop : MonoBehaviour{

	public readonly Vector2 dropSize = new Vector2(1f, 1f);
	public readonly byte dropForceMultiplikator = 1;
	private static readonly char seperator = '|';

	public Rigidbody2D DropRigidbody { get; private set; }
	public SpriteRenderer SpriteRenderer{ get; private set; }
	public BoxCollider2D BoxCollider2D { get; private set; }
	
	public uint ItemId{ get => itemId; set{
		itemId = value;
		if(SpriteRenderer != null)
			SpriteRenderer.sprite = ItemAssets.Singleton.GetSpriteFromItemID(itemId);
		} 
	}
	[SerializeField]
	private uint itemId;

	public ushort Count{ get => count; set{
		count = value;
			if(count == 0)
				Destroy(gameObject);
		} 
	}
	private ushort count = 1;

	

	private void Awake() {
		SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		if(itemId !=0)
			SpriteRenderer.sprite = ItemAssets.Singleton.GetSpriteFromItemID(itemId);
		SpriteRenderer.size = dropSize;

		DropRigidbody = gameObject.AddComponent<Rigidbody2D>();
		DropRigidbody.mass = 0.5f;
		DropRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		DropRigidbody.gravityScale = 0;
		
		BoxCollider2D = gameObject.AddComponent<BoxCollider2D>();
		BoxCollider2D.size = dropSize;
	}
	private void Start() {
		DropRigidbody.AddForce(new Vector2(new Random().Next(-5, 5)/10f, 1) * dropForceMultiplikator, ForceMode2D.Impulse);
		DropRigidbody.gravityScale = 1; 
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if(collision.transform.CompareTag("Player"))
			PickUp(collision.gameObject);
		if(collision.gameObject.layer == gameObject.layer){
			if(collision.gameObject.TryGetComponent(out Drop other)) {
				if(other.itemId == itemId) {
					Count += other.Count;
					other.Count = 0;
				}
			} else
				Debug.LogWarning($"Drop Object has no Drop.cs! {collision.gameObject.name}");
		}
	}

	public void PickUp(GameObject playerGO){
		playerGO.gameObject.GetComponentInChildren<Inventory>().AddItem(ItemId, Count, out ushort itemCountNotAdded);
		if(itemCountNotAdded > Count)
			throw new Exception();
		Count = itemCountNotAdded;
    }

    public override string ToString() {
		Vector3 pos = gameObject.transform.position ;
		return $"{itemId}{seperator}{Count}{seperator}{pos.x},{pos.y}";
    }

	public static Tuple<uint, ushort, Vector2> ConvertFromString(string s){
		string[] args = s?.Split(seperator) ?? throw new ArgumentNullException();

		if(args.Length != 3)
			throw new ArgumentException($"String has not 3 Elements: {args.Length}");

		if(!uint.TryParse(args[0], out uint itemID))
			throw new ArgumentException($"{args[0]} not uint");
		if(!ushort.TryParse(args[1],  out ushort count))
			throw new ArgumentException($"{args[1]} not ushort");

		string[] posArgs = args[2].Split(',');

		if(args.Length != 2)
			throw new ArgumentException($"Positionstring has not 2 Elements: {args.Length}");

		if(!float.TryParse(posArgs[0], out float x))
			throw new ArgumentException($"{posArgs[0]} not float");
		if(!float.TryParse(posArgs[1], out float y))
			throw new ArgumentException($"{posArgs[1]} not float");

		return new Tuple<uint, ushort, Vector2>(itemID, count, new Vector2(x, y));
    }
}