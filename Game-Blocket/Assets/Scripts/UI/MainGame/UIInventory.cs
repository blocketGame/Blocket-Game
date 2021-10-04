using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the Changable Settings for the Main-Game UI
/// </summary>
public class UIInventory : MonoBehaviour
{
	#region General Settings
	[Header("Settings: Changable - Backgrounds")]
	public Color inventoryBackground;
	/// <summary>Space to Edge of the GameObject</summary>
	public int spaceToBorderX, spaceToBorderY;
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
	public Text titleText, descitonText;
	#endregion

	#region Static Resources !DO NOT TOUCH!
	[Header("Static: General")]
	///<summary>Gameobject from Inspector</summary>
	public GameObject uiParent, slotField;
	/// <summary>Image from Inspector</summary>
	public Image inventoryBackgroundImage;
	/// <summary>Prefab from Inspector</summary>
	public GameObject prefabItemSlot;
	#endregion

	#region Initzializement

	/// <summary>
	/// Initzialize the Inventory;
	/// </summary>
	private void InitUI() {
		InitSlots();
		//InitPlayerInfo();

		///Initzialize the Inventory class;
		///
	}
	/// <summary>
	/// Initzialize the PlayerInfoUI
	/// </summary>
	private void InitPlayerInfo() {
		//TODO: Make dynamic
		///ArmorSlots
		//foreach(GameObject go1 in armorSlots)
		//	go1.transform.localPosition = new Vector3Int(spaceToBorderX + (int)go1.transform.localPosition.x, (int)go1.transform.localPosition.y, 1);

		///AccessoiresSlot
		if(!accessoiresParent)
			Debug.LogError("AccessoiresParent not Initzialized");
		for(int i = 0; i < countAccessoireSlots; i++) {
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
		GlobalVariables.inventory.invSlots = new List<UIInventorySlot>();
		//Get With and height from the Prefab
		float prefW = prefabItemSlot.GetComponent<RectTransform>().rect.width,
			prefH = prefabItemSlot.GetComponent<RectTransform>().rect.height;
		//Go through every Slot
		for(byte a = 0; a < rows; a++) {
			for(byte b = 0; b < coloums; b++) {
				//Calc the !absolute Pos
				float itemSlotX = slotField.transform.position.x + prefW * b + spaceToBorderX + colspacingInvSlot * b;
				float itemSlotY = slotField.transform.position.y - (prefH * a + spaceToBorderY + rowspacingInvSlot * a);
				//Instantiate the Gameobject
				GameObject itemSlot = Instantiate(prefabItemSlot, new Vector3Int((int)(itemSlotX), (int)(itemSlotY), 1), Quaternion.identity, slotField.transform);
				//Name it
				itemSlot.name = $"Slot {a} - {b}";
				//Add to Inventory Logic
				GlobalVariables.inventory.invSlots.Add(itemSlot.GetComponent<UIInventorySlot>());
			}
		}
	}


	#endregion

	/// <summary>"Reload" at the beginning</summary>
	public void Awake() {
		ReloadSettings();
		InitUI();
		GlobalVariables.inventory.armorSlots = armorSlots;
		GlobalVariables.inventory.accessoiresSlots = accessoiresSlots;
		InventoryOpened = false;
	}

	/// <summary>Reloads all UI Settings</summary>
	public void ReloadSettings() {
		if(!uiParent)
			Debug.LogError("Parent from UI is NULL!");
		//Set the Backgroung from the UI-Inventory
		inventoryBackgroundImage.color = inventoryBackground;
	}

	/// <summary>Returns and sets if the inventory should open</summary>
	public static bool InventoryOpened {
		get {
			return inventoryOpened;
		}
		set {
			inventoryOpened = value;
			if(value) {
				//TODO: Optional things to do... Example: Lock Mouseplace or break
				GameObject.Find("Inventory").gameObject.SetActive(true);
			} else {
				//TODO: Optional things to do...
				GameObject.Find("Inventory").gameObject.SetActive(false);
			}

		}
	}
	private static bool inventoryOpened;
}
