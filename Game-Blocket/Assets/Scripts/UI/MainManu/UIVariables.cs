using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the Changable Settings for the Main-Game UI
/// </summary>
public class UIInventory : MonoBehaviour
{
	#region General Settings
	[Header("Changable - Backgrounds")]
	public Color inventoryBackground;
	#endregion

	#region InventorySlotSettings
	[Header("Slots - Inventory")]
	/// <summary>Count of Rows and Coloums</summary>
	public byte rows = 5, coloums = 8;

	/// <summary>Space between Rows and Colums</summary>
	public int rowspacing = 15, colspacing = 15;
	
	/// <summary>Space to Edge of the GameObject</summary>
	public int spaceToBorderX = 30, spaceToBorderY = 30;
	#endregion

	#region Static Resources !DO NOT TOUCH!
	[Header("Static")]
	public GameObject uiParent, slotField;
	public Image inventoryBackgroundImage;
	public GameObject prefabItemSlot;
	#endregion

	#region Initzializement

	/// <summary>
	/// Initzialize the Inventory;
	/// </summary>
	private void InitUI() {
		InitSlots();
	}

	/// <summary>
	/// Initzialize the ItemSlotField
	/// </summary>
	private void InitSlots() {
		//Get With and height from the Prefab
		float prefW = prefabItemSlot.GetComponent<RectTransform>().rect.width,
			prefH = prefabItemSlot.GetComponent<RectTransform>().rect.height;
		//Go through every Slot
		for(byte a = 0; a < rows; a++) {
			for(byte b = 0; b < coloums; b++) {
				//Calc the !absolute Pos
				float itemSlotX = slotField.transform.position.x + prefW * b + spaceToBorderX + colspacing * b;
				float itemSlotY = slotField.transform.position.y - (prefH * a + spaceToBorderY + rowspacing * a);
				//Instantiate the Gameobject
				GameObject itemSlot = Instantiate(prefabItemSlot, new Vector3Int((int)(itemSlotX), (int)(itemSlotY), 1), Quaternion.identity, slotField.transform);
				//Name it
				itemSlot.name = $"Slot {a} - {b}";
			}
		}
	}


	#endregion

	/// <summary>
	/// Reload at the beginning
	/// </summary>
	public void Awake() {
		ReloadSettings();
		InitUI();
		InventoryOpened = false;
	}

	/// <summary>
	/// Reloads all UI Settings
	/// </summary>
	public void ReloadSettings() {
		if(!uiParent)
			Debug.LogError("Parent from UI is NULL!");
		//Set the Backgroung from the UI-Inventory
		inventoryBackgroundImage.color = inventoryBackground;
	}

	/// <summary>
	/// Returns and sets if the inventory should open
	/// </summary>
	public bool InventoryOpened {
		get {
			return inventoryOpened;
		}
		set {
			inventoryOpened = value;
			if(value) {
				//TODO: Optional things to do... Example: Lock Mouse place or break
				uiParent.transform.Find("Inventory").gameObject.SetActive(true);
			} else {
				//TODO: Optional things to do...
				uiParent.transform.Find("Inventory").gameObject.SetActive(false);
			}

		}
	}
	private bool inventoryOpened;
}
