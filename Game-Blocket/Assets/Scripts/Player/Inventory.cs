using System;
using System.Collections.Generic;

using UnityEngine;

using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// <b>Inventory Logic</b><br></br>
/// Feel free  to use it!
/// </summary>
public class Inventory : MonoBehaviour {
	public static Inventory Singleton { get; private set; }

	/// <summary>List of ArmorSlots in the inventory => <seealso cref="UIInventorySlot"/><br></br>[0]->Head,</summary>
	public List<UIInventorySlot> ArmorSlots { get; set; }
	/// <summary>List of acessoiresslots in the inventory => <seealso cref="UIInventorySlot"/></summary>
	public List<UIInventorySlot> AccessoiresSlots { get; set; }
	/// <summary>List of the inventory Slots => <seealso cref="UIInventorySlot"/></summary>
	public List<UIInventorySlot> InvSlots { get; } = new List<UIInventorySlot>();

	/// <summary>List of the Hud Slots</summary>
	public List<UIInventorySlot> HudSlots { get; } = new List<UIInventorySlot>();

	public uint SelectedItemId => InvSlots[SelectedSlot]?.ItemID ?? 0;
	public Item SelectedItemObj => SelectedItemId == 0 ? null : ItemAssets.Singleton.GetItemFromItemID(SelectedItemId);

	public byte SelectedSlot { get => _selectedSlot; set {
			if (value >= HudSlots.Count || value < 0)
				throw new ArgumentException($"Hud-Slot Nr.: {value} not found!");
			HudSlots[_selectedSlot].IsSelected = false;
			_selectedSlot = value;
			HudSlots[_selectedSlot].IsSelected = true;
			PlayerVariables.Singleton.ReloadItemInHand();
		}
	}
	private byte _selectedSlot = 0;

	/// <summary>Last slot active pressed</summary>
	[HideInInspector]
	public UIInventorySlot atHand;
	//[HideInInspector]
	public Vector2 atHandVector;

	/// <summary>
	/// Event if the User presses a slot
	/// </summary>
	/// <param name="slotPressed">Slot that was pressed by the local user</param>
	public void PressedSlot(UIInventorySlot slotPressed) {
		//Equipable
		if(slotPressed.type != EquipableItem.EquipableType.None && atHand.ItemID != 0)
			if(!(ItemAssets.Singleton.GetItemFromItemID(atHand.ItemID) is EquipableItem eI && eI.type == slotPressed.type))
				return;

		uint temp = atHand.ItemID;
		ushort iCT = atHand.ItemCount;
		/*if (Input.GetKey(KeyCode.Mouse1))
		{
			atHand.ItemCount--;
			slotPressed.ItemCount++;
			slotPressed.ItemID = temp;
		}
		else if (Input.GetKey(KeyCode.Mouse0))
		{*/
			atHand.ItemID = slotPressed.ItemID;
			atHand.ItemCount = slotPressed.ItemCount;

			slotPressed.ItemCount = iCT;
			slotPressed.ItemID = temp;

		//}
		atHand.gameObject.SetActive(atHand.ItemID != 0);
		UIInventory.Singleton.SynchronizeToHotbar();
	}

	private void Awake() => Singleton = this;        

    public void Update() {
		if (atHand?.ItemID != null)
			atHand.transform.position = atHandVector + new Vector2(Input.mousePosition.x, Input.mousePosition.y);

	}

	/// <summary>
	/// Adds an <see cref="Item"/> into the inventory
	/// </summary>
	/// <param name="itemToAdd">The Item Object</param>
	/// <returns>True if the Item </returns>
	[Obsolete]
	public bool AddItem(Item itemToAdd) => AddItem(itemToAdd, 1, out _);

	public bool AddItem(uint itemIDToAdd, ushort itemCount, out ushort itemCountNotAdded) {
		Item item = ItemAssets.Singleton.GetItemFromItemID(itemIDToAdd) ?? throw new ArgumentException($"No Item found! ID:{itemIDToAdd}");
		bool x = AddItem(item, itemCount, out ushort i);
		itemCountNotAdded = i;
		return x;
	}

	public bool AddItem(Item itemToAdd, ushort itemCount, out ushort itemCountNotAdded){
		bool x = AddItemInnerMethod(itemToAdd, itemCount, out ushort i);
		itemCountNotAdded = i;
		UIInventory.Singleton.SynchronizeToHotbar();
		return x;
	}

	/// <summary
	/// Adds some <see cref="Item"/>s into the inventory
	/// </summary>
	/// <param name="itemToAdd">Item to Add</param>
	/// <param name="itemCount">Number of Item`s</param>
	/// <param name="itemCountNotAdded"></param>
	/// <returns><see langword="true"/> if the <see cref="Item"/> has been added</returns>
	/// 
	private bool AddItemInnerMethod(Item itemToAdd, ushort itemCount, out ushort itemCountNotAdded) {
		UIInventorySlot wannaAddThere = null;
		///If item has ItemType: <see cref="Item.ItemType.SINGLE"/>
		if (itemToAdd.itemType == Item.ItemType.SINGLE) {
			wannaAddThere = GetNextFreeSlot();
			if (wannaAddThere) {
				wannaAddThere.ItemID = itemToAdd.id;
				itemCountNotAdded = 0;
				return true;
			}
			itemCountNotAdded = itemCount;
			return false;
		}

		///If item has ItemType: <see cref="Item.ItemType.STACKABLE"/>
		ushort toAddNow = itemCount;
		while (toAddNow > 0) {
			//Try to find a slot with the same Item
			foreach (UIInventorySlot inventorySlotNow in FindItem(itemToAdd))
				if (inventorySlotNow.ItemCount <= GlobalVariables.maxItemCountForMultiple) {
					wannaAddThere = inventorySlotNow;
					break;
				}
			//If no slot with this item is found => get a new one
			if (!wannaAddThere)
				wannaAddThere = GetNextFreeSlot();

			//Add the Item if a slot has been found
			if (wannaAddThere)
			{
				if (wannaAddThere.ItemID == 0)
				{
					wannaAddThere.ItemID = itemToAdd.id;
					wannaAddThere.ItemCount = toAddNow;
					toAddNow = 0;
				}else{
					ushort iCBefore = wannaAddThere.ItemCount;
					if (iCBefore + toAddNow > GlobalVariables.maxItemCountForMultiple)
					{
						ushort iCAddable = (ushort)(GlobalVariables.maxItemCountForMultiple - iCBefore);
						toAddNow -= iCAddable;
						wannaAddThere.ItemCount = GlobalVariables.maxItemCountForMultiple;
					}
					else
					{
						wannaAddThere.ItemCount += toAddNow;
						toAddNow = 0;
					}
				}
			}
			else
				break;
		}
		itemCountNotAdded = toAddNow;
		return toAddNow != itemCount;
	}

	/// <summary>
	/// Goes through all slots and returns the <see cref="UIInventorySlot"/> of one specific item
	/// </summary>
	/// <param name="itemToFind">The item to find</param>
	/// <returns>List off all Slots with a specific Item</returns>
	public List<UIInventorySlot> FindItem(Item itemToFind) {
		List<UIInventorySlot> itemSlotsFound = new List<UIInventorySlot>();
		foreach (UIInventorySlot invSlotNow in InvSlots)
			if (invSlotNow.ItemID == itemToFind.id)
				itemSlotsFound.Add(invSlotNow);
		return itemSlotsFound;
	}

	/// <summary>
	/// TODO: Change too variable
	/// 
	/// Goes through inventory and searches the first free slot
	/// </summary>
	/// <returns>First free slot<br></br>Null if no slot is free</returns>
	public UIInventorySlot GetNextFreeSlot() {
		foreach (UIInventorySlot invSlotNow in InvSlots)
			if (invSlotNow.ItemID == 0)
				return invSlotNow;
		return null;
	}

	/// <summary>
	/// Goes through the Inventory and searches the first of an Item
	/// </summary>
	/// <param name="itemtToFind">Item that has to be found</param>
	/// <returns>Null if no Item has been found<br></br>An <see cref="UIInventorySlot"/> with the Item to find</returns>
	public UIInventorySlot FindFirstItem(Item itemtToFind) {
		foreach (UIInventorySlot invSlotNow in InvSlots)
			if (invSlotNow.ItemID == itemtToFind.id)
				return invSlotNow;
		return null;
	}

	/// <summary>
	/// Checks if a specific value of an Item can be removed
	/// </summary>
	/// <param name="itemToRemove">Item to Remove</param>
	/// <param name="countToRemove">Number of the Item to be removed</param>
	/// <returns>True if the Inventory has enough of the specific item</returns>
	public bool CanBeRemoved(Item itemToRemove, ushort countToRemove) {
		return GetItemCountFromType(itemToRemove) >= countToRemove;
	}

	/// <summary>
	/// Removes an Amount of Items<br></br>
	/// Before it checks if it can be removed
	/// </summary>
	/// <param name="itemToRemove">The <see cref="Item"/> to remove</param>
	/// <param name="countToRemove">Number of items</param>
	/// <returns>True if has been removed; false if it cannot be removed due to <see cref="NullReferenceException"/> or more count than items available</returns>
	public bool RemoveItem(Item itemToRemove, ushort countToRemove) {
		if (!(itemToRemove != null && CanBeRemoved(itemToRemove, countToRemove)))
			return false;
		UIInventorySlot slotToRemove = FindFirstItem(itemToRemove);
		if (slotToRemove.ItemCount < countToRemove) {
			int toRemoveAfter = countToRemove - slotToRemove.ItemCount;
			slotToRemove.ItemID = 0;
			RemoveItem(itemToRemove, (ushort)toRemoveAfter);
		} else if (slotToRemove.ItemCount == countToRemove)
			slotToRemove.ItemID = 0;
		else
			slotToRemove.ItemCount -= countToRemove;
		UIInventory.Singleton.SynchronizeToHotbar();
		return true;
	}
	/// <summary>
	/// Get the Number of one ItemType
	/// </summary>
	/// <param name="itemToFind">The item which you want to find</param>
	/// <returns>the numver of items</returns>
	public ushort GetItemCountFromType(Item itemToFind) {
		ushort sum = 0;
		foreach (UIInventorySlot slot in FindItem(itemToFind))
			sum += slot.ItemCount;
		return sum;
	}
}