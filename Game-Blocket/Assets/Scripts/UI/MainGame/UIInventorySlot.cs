using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TW: Used for MainMenu-Scene<br></br>
/// 
/// Handles the <see cref="Button"/>-Click-Events
/// 
/// </summary>
public class UIInventorySlot : MonoBehaviour {
    #region Static Resources
    /// <summary><see cref="global::UIInventory"/> </summary>
    public UIInventory UIInventory => GlobalVariables.UIInventory;
	/// <summary><see cref="Text"/></summary>
	public Text textDown;
	/// <summary><see cref="Button"/>-Button</summary>
	public Button button;
	/// <summary><see cref="Image"/>-Button</summary>
	public Image itemImage, backgroundImage;
	/// <summary>Image Sprites which are swapable</summary>
	public Sprite imgActive, imagInactive, defaultSprite;
	/// <summary>Checks Whether or not this Slot is just a copy</summary>
	public bool isHotBarSlot;
	/// <summary>InventorySlots parent => this Slot will be the copy for Hotbar <summary>
	public UIInventorySlot parent;
	#endregion

	public readonly bool useOldPointerhandling = true;

	public bool IsSelected { get => _isSelected; set {
			_isSelected = value;
			backgroundImage.color = value ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
		} }
	private bool _isSelected;

	/// <summary><see cref="ItemID"/></summary>
	public uint ItemID {
		get => _itemId;
		set {
			_itemId = value;
			ItemObject = GlobalVariables.ItemAssets?.GetItemFromItemID(value);
			if (value == 0)
				ItemCount = 0;
			ReloadSlot();
		}
	}
	private uint _itemId;

	public ushort ItemCount {
		get => _itemCount;
		set {
			_itemCount = value;
			ReloadSlot();
		}
	}
	private ushort _itemCount = 0;

	private Item ItemObject { get; set; }

	/// <summary>Reloads the Itemslot<br></br><b>Be carfull when deleting!</b></summary>
	public void ReloadSlot() {
		
		itemImage.sprite = ItemObject?.itemImage;
		itemImage.sprite ??= defaultSprite;
		//Hide counttext if item is Single type
		if (ItemObject != null) {
			textDown.gameObject.SetActive(ItemObject.itemType == Item.ItemType.STACKABLE);
			//Write itemCount into the texfield
			textDown.color = Color.white;
			//textDown.gameObject.transform.position.Set(textDown.gameObject.transform.position.x,textDown.gameObject.transform.position.y + 100, textDown.gameObject.transform.position.z);
			textDown.text = string.Empty + _itemCount;
		}
		itemImage.gameObject.SetActive(ItemObject != null);
		textDown.gameObject.SetActive(ItemObject != null);
	}

	private bool _active;
	public bool Active {
		get { return _active; }
		set {
			if (value) {

				UIInventory.DescriptionText = ItemObject?.description ?? string.Empty;
				UIInventory.TitleText = ItemObject?.name ?? string.Empty;
			}
			_active = value;
		}
	}

	public void Awake() {

		button ??= GetComponentInChildren<Button>();

		if (useOldPointerhandling)
			button.onClick.AddListener(() => {
				Debug.Log("Pressed!");
				GlobalVariables.Inventory.PressedSlot(this); 
			});
        else
        {
			button.gameObject.AddComponent<SlotOptionsScript>();
			button.gameObject.GetComponent<SlotOptionsScript>().invSlot = this;
			if (UIInventory != null)
				button.gameObject.GetComponent<SlotOptionsScript>().SlotOptions = UIInventory._slotOptions;
        }
	}

	public void OnMouseOver() {
		Debug.Log("A");
	}
}