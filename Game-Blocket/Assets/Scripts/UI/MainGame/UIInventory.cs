using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// Sets the Changable Settings for the Main-Game UI
/// </summary>
public class UIInventory : MonoBehaviour {
	#region General Settings
	[Header("Settings: Changable - Backgrounds")]
	public Color inventoryBackground;
	/// <summary>Space to Edge of the GameObject</summary>
	public int spaceToBorderX, spaceToBorderY;
	public GameObject atHandSlot;
	#endregion

	#region InventorySlotSettings
	[Header("Settings: Slots - Inventory")]
	/// <summary>Count of Rows and Coloums</summary>
	public byte rows = 5, coloums = 8;

	/// <summary>Space between Rows and Colums</summary>
	public int rowspacingInvSlot = 15, colspacingInvSlot = 15;


	#endregion

	#region InventoryPlayerInfoSettings
	[Header("Settings: Playerinfo - Inventory")]
	public byte countAccessoireSlots, rowspacingPlayerInfo;

	///<summary>
	///AccessoiresSlots are inherted from the <see cref="prefabItemSlot"/> thats why they have to be scaled smaller<br></br>
	///Min: 1, Max: 100 in percent
	/// </summary>
	[Range(1, 100)]
	public byte scaleIndicator;

	[Header("Static: Playerinfo - Inventory")]
	///<summary>ArmorSlots: Top - Mid - Down</summary>
	public List<UIInventorySlot> armorSlots = new List<UIInventorySlot>(3), accessoiresSlots = new List<UIInventorySlot>();
	public GameObject accessoiresParent;
	#endregion

	#region InventoryPlayerStatsSettings
	[Header("Static: Player Stats")]
	public Text heartStat, shieldStat, swordStat;
	[Header("Static: Description")]
	public Text titleText, descriptonText;
	#endregion

	#region Static Resources !DO NOT TOUCH!
	[Header("Static: General")]
	///<summary>Gameobject from Inspector</summary>
	public GameObject uiParent, slotField, uiHud, hudslotfieldParent;
	/// <summary>Image from Inspector</summary>
	public Image inventoryBackgroundImage;
	/// <summary>Prefab from Inspector</summary>
	public GameObject prefabItemSlot;
	/// <summary>ItemAssets - Prefab </summary>
	public ItemAssets itemAssets;
	/// <summary><see cref="Inventory"/></summary>
	private Inventory _inventory;
	public GameObject _slotOptions;
	#endregion

	#region Initzializement

	/// <summary>
	/// Initzialize the Inventory;
	/// </summary>
	private void InitUI() {
		InitSlots();
		InitHudSlots();
		//InitPlayerInfo();
		InitAtHand();
		itemAssets = GlobalVariables.GlobalAssets.GetComponent<ItemAssets>();
		if (!itemAssets)
			Debug.LogException(new NullReferenceException("Item Assets not found!"));
		/*if(GlobalVariables.itemTest)
			foreach(Item i in itemAssets.BlockItemsInGame)
				_inventory.AddItem(i);*/
	}

	/// <summary>
	/// Initzialize the AtHand<see cref="UIInventorySlot"/>
	/// </summary>
	private void InitAtHand() {
		atHandSlot = Instantiate(prefabItemSlot, GameObject.Find("Inventory").transform);
		atHandSlot.name = "SlotAtHand";
		atHandSlot.SetActive(false);
		Destroy(atHandSlot.GetComponentInChildren<Image>());
		Destroy(atHandSlot.GetComponentInChildren<Button>());

		UIInventorySlot atHandUISlot = atHandSlot.GetComponent<UIInventorySlot>();
		_inventory.atHand = atHandUISlot;
		atHandUISlot.itemImage.raycastTarget = false;

		RectTransform atHandT = atHandSlot.GetComponent<RectTransform>();
		atHandT.localScale = new Vector3(0.8f, 0.8f, 1);
		_inventory.atHandVector = new Vector2(-atHandT.rect.width / 2, atHandT.rect.height / 2);
	}

	/// <summary>
	/// Initzialize the PlayerInfoUI
	/// <br></br>Not used!
	/// </summary>
	private void InitPlayerInfo() {
		//TODO: Make dynamic
		///ArmorSlots
		//foreach(GameObject go1 in armorSlots)
		//	go1.transform.localPosition = new Vector3Int(spaceToBorderX + (int)go1.transform.localPosition.x, (int)go1.transform.localPosition.y, 1);

		///AccessoiresSlot
		if (!accessoiresParent)
			Debug.LogError("AccessoiresParent not Initzialized");
		for (int i = 0; i < countAccessoireSlots; i++) {
			float height = prefabItemSlot.GetComponent<RectTransform>().rect.height * scaleIndicator / 100;
			float accSlotY = accessoiresParent.transform.position.y + (height * i + rowspacingPlayerInfo * i);
			Vector3 posSlotNow = new Vector3(accessoiresParent.transform.position.x, accSlotY, 1);
			GameObject aGo = Instantiate<GameObject>(prefabItemSlot, posSlotNow, Quaternion.identity, accessoiresParent.transform);
			RectTransform aGoRT = aGo.GetComponent<RectTransform>();
			aGoRT.localScale = new Vector3(scaleIndicator / 100, scaleIndicator / 100, 1);
			aGo.name = $"Accessoire Slot {i}";
			//accessoiresSlots.Add(aGo);
		}
	}

	/// <summary>
	/// Initzialize the ItemSlotField
	/// </summary>
	private void InitSlots() {
		//Get With and height from the Prefab
		float prefW = prefabItemSlot.GetComponent<RectTransform>().rect.width,
			prefH = prefabItemSlot.GetComponent<RectTransform>().rect.height;
		//Go through every Slot
		for (byte a = 0; a < rows; a++) {
			for (byte b = 0; b < coloums; b++) {
				//Calc the !absolute Pos
				float itemSlotX = slotField.transform.position.x + prefW * b + spaceToBorderX + colspacingInvSlot * b;
				float itemSlotY = slotField.transform.position.y - (prefH * a + spaceToBorderY + rowspacingInvSlot * a);
				//Instantiate the Gameobject
				GameObject itemSlot = Instantiate(prefabItemSlot, new Vector3Int((int)(itemSlotX), (int)(itemSlotY), 1), Quaternion.identity, slotField.transform);
				//Name it
				itemSlot.name = $"Slot {a} - {b}";
				//Add to Inventory Logic
				_inventory.InvSlots.Add(itemSlot.GetComponent<UIInventorySlot>());
			}
		}
	}

	/// <summary></summary>
	private void InitHudSlots() {
		//Get With and height from the Prefab
		float prefW = prefabItemSlot.GetComponent<RectTransform>().rect.width;
		//Go through every Slot
		for (byte a = 0; a < coloums; a++) {
			//Calc the !absolute Pos
			float itemSlotX = hudslotfieldParent.transform.position.x + prefW * a + 50 + colspacingInvSlot * 0.2f * a;
			float itemSlotY = hudslotfieldParent.transform.position.y - 15;
			//Instantiate the Gameobject
			GameObject itemSlot = Instantiate(prefabItemSlot, new Vector3Int((int)(itemSlotX), (int)(itemSlotY), 1), Quaternion.identity, hudslotfieldParent.transform);
			itemSlot.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1);
			//Name it
			itemSlot.name = $"HudSlot {a}";
			//Specify that it is a HotbarSlot
			itemSlot.GetComponent<UIInventorySlot>().isHotBarSlot = true;
			itemSlot.GetComponent<UIInventorySlot>().parent = _inventory.InvSlots[a];
			//Add to Inventory Logic
			_inventory.HudSlots.Add(itemSlot.GetComponent<UIInventorySlot>());
		}
		_inventory.SelectedSlot = 0;
	}
	#endregion

	#region UnityMethods
	/// <summary>"Reload" at the beginning</summary>
	public void Awake() {
		GlobalVariables.PlayerVariables.uIInventory = this;
		GlobalVariables.PlayerVariables.healthScript = GetComponentInChildren<HealthScript>();
		GlobalVariables.PlayerVariables.MaxHealth = GlobalVariables.PlayerVariables.MaxHealth;
		GlobalVariables.PlayerVariables.Health = GlobalVariables.PlayerVariables.Health;
		name = "UI";
		_inventory = GlobalVariables.Inventory;
		if (_inventory == null)
			Debug.LogError("Inventory not found!");
		ReloadSettings();
		InitUI();
		_inventory.ArmorSlots = armorSlots;
		_inventory.AccessoiresSlots = accessoiresSlots;
		InventoryOpened = false;
	}


	public void Update() {
		if (Input.GetKeyDown(GameManager.SPNow.Keys["InventoryKey"])) {
			InventoryOpened = !InventoryOpened;
			if (!InventoryOpened)
				SynchronizeToHotbar();
			else
				SynchronizeToInv();
			uiHud.SetActive(!InventoryOpened);

		}
		if (Input.mouseScrollDelta.y != 0) {
			float val = Input.mouseScrollDelta.y;
			if (val < 0)
				if(_inventory.SelectedSlot == _inventory.HudSlots.Count - 1)
					_inventory.SelectedSlot = 0;
				else
					_inventory.SelectedSlot += 1;
			else
				if (_inventory.SelectedSlot == 0)
					_inventory.SelectedSlot = (byte)(_inventory.HudSlots.Count - 1);
				else
					_inventory.SelectedSlot -= 1;
		}
	}
	#endregion

	/// <summary>Reloads all UI Settings</summary>
	public void ReloadSettings() {
		if (!uiParent)
			Debug.LogError("Parent from UI is NULL!");
		//Set the Backgroung from the UI-Inventory
		inventoryBackgroundImage.color = inventoryBackground;
	}

	/// <summary>Returns and sets if the inventory should open</summary>
	public bool InventoryOpened {
		get {
			return inventoryOpened;
		}
		set {
			inventoryOpened = value;
			if (value) {
				//TODO: Optional things to do... Example: Lock Mouseplace or break
				uiParent.SetActive(true);
			} else {
				//TODO: Optional things to do...
				uiParent.SetActive(false);
			}

		}
	}
	private static bool inventoryOpened;

	public string DescriptionText {
		get { return descriptonText.text; }
		set { descriptonText.text = value; }
	}

	public string TitleText {
		get { return titleText.text; }
		set { titleText.text = value; }
	}

	#region Bidirectional Sync

	/// <summary>
	/// Synchronizes Hotbar State of Slots Row 1
	/// </summary>
	public void SynchronizeToInv() {
		foreach (UIInventorySlot sl in _inventory.InvSlots) {
			if (sl.isHotBarSlot) {
				///[TODO]
				foreach (UIInventorySlot sl1 in _inventory.InvSlots) {
					if (!sl1.isHotBarSlot && sl.parent.name.Equals(sl1.name)) {
						sl1.ItemID = sl.ItemID;
						sl1.ItemCount = sl.ItemCount;
					}
				}
			}
		}
	}
	/// <summary>
	/// Synchronizes Inventory State of Slots Row 1
	/// </summary>
	public void SynchronizeToHotbar() {
		foreach (UIInventorySlot sl in _inventory.InvSlots) {
			if (sl.isHotBarSlot) {
				///[TODO]
				foreach (UIInventorySlot sl1 in _inventory.InvSlots) {
					if (!sl1.isHotBarSlot && sl.parent.name.Equals(sl1.name)) {
						sl.ItemID = sl1.ItemID;
						sl.ItemCount = sl1.ItemCount;
					}
				}
			}
		}
	}
	#endregion

}