using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// <b>Inventory Logic</b><br></br>
/// Feel free  to use it!
/// </summary>
public class Inventory : MonoBehaviour{
	/// <summary>List of ArmorSlots in the inventory => <seealso cref="UIInventorySlot"/><br></br>[0]->Head,</summary>
	public List<UIInventorySlot> ArmorSlots { get; set; }
	/// <summary>List of acessoiresslots in the inventory => <seealso cref="UIInventorySlot"/></summary>
	public List<UIInventorySlot> AccessoiresSlots { get; set; }
	/// <summary>List of the inventory Slots => <seealso cref="UIInventorySlot"/></summary>
	public List<UIInventorySlot> InvSlots { get; } = new List<UIInventorySlot>();

	/// <summary>Last slot active pressed</summary>
	public UIInventorySlot atHand;
	public Vector2 atHandVector;

	public void PressedSlot(UIInventorySlot slotPressed) {
		Item temp = atHand.Item;
		atHand.Item = slotPressed.Item;
		slotPressed.Item = temp;
		atHand.gameObject.SetActive(atHand.Item != null);
	}

	public void Update() {
		if(atHand.Item != null)
			atHand.transform.position = atHandVector + new Vector2(Input.mousePosition.x, Input.mousePosition.y);
	}

	/// <summary>
	/// TODO: Add num of count<br></br>
	/// Adds an <see cref="Item"/> into the inventory
	/// </summary>
	/// <param name="itemToAdd">Item to Add</param>
	/// <returns><see langword="true"/> if the <see cref="Item"/> has been added</returns>
	public bool AddItem(Item itemToAdd) {
		UIInventorySlot wannaAddThere = null;

		///If item has ItemType: <see cref="Item.ItemType.SINGLE"/>
		if(itemToAdd.itemType == Item.ItemType.SINGLE) {
			wannaAddThere = GetNextFreeSlot();
			if(wannaAddThere) {
				wannaAddThere.Item = itemToAdd;
				return true;
			}
			return false;
		}

		///If item has ItemType: <see cref="Item.ItemType.STACKABLE"/>
		//Try to find a slot with the same Item
		foreach(UIInventorySlot inventorySlotNow in FindItem(itemToAdd))
			if(inventorySlotNow.ItemCount <= GlobalVariables.maxItemCountForMultiple) {
				wannaAddThere = inventorySlotNow;
				break;
			}
		//If no slot with this item is found => get a new one
		if(!wannaAddThere)
			wannaAddThere = GetNextFreeSlot();
		//Add the Item if a slot has been found
		if(wannaAddThere) {
			if(wannaAddThere.Item == null)
				wannaAddThere.Item = itemToAdd;
			else
				wannaAddThere.ItemCount++;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Goes through all slots and returns the <see cref="UIInventorySlot"/> of one specific item
	/// </summary>
	/// <param name="itemToFind">The item to find</param>
	/// <returns>List off all Slots with a specific Item</returns>
	public List<UIInventorySlot> FindItem(Item itemToFind) {
		List<UIInventorySlot> itemSlotsFound = new List<UIInventorySlot>();
		foreach(UIInventorySlot invSlotNow in InvSlots)
			if(invSlotNow.Item == itemToFind)
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
		foreach(UIInventorySlot invSlotNow in InvSlots)
			if(invSlotNow.Item == null)
				return invSlotNow;
		return null;
	}

	/// <summary>
	/// Goes through the Inventory and searches the first of an Item
	/// </summary>
	/// <param name="itemtToFind">Item that has to be found</param>
	/// <returns>Null if no Item has been found<br></br>An <see cref="UIInventorySlot"/> with the Item to find</returns>
	public UIInventorySlot FindFirstItem(Item itemtToFind) {
		foreach(UIInventorySlot invSlotNow in InvSlots)
			if(invSlotNow.Item == itemtToFind)
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
		return GetItemCountFromType(itemToRemove) > countToRemove;
	}

	/// <summary>
	/// Removes an Amount of Items<br></br>
	/// Bevore it checks if it can be removed
	/// </summary>
	/// <param name="itemToRemove">The <see cref="Item"/> to remove</param>
	/// <param name="countToRemove">Number of items</param>
	/// <returns>True if has been removed; false if it cannot be removed due to <see cref="NullReferenceException"/> or more count than items available</returns>
	public bool RemoveItem(Item itemToRemove, ushort countToRemove) {
		if(!(itemToRemove != null && CanBeRemoved(itemToRemove, countToRemove)))
			return false;
		UIInventorySlot slotToRemove = FindFirstItem(itemToRemove);
		if(slotToRemove.ItemCount < countToRemove) {
			int toRemoveAfter = countToRemove - slotToRemove.ItemCount;
			slotToRemove.Item = null;
			RemoveItem(itemToRemove, (ushort)toRemoveAfter);
		} else if(slotToRemove.ItemCount == countToRemove)
			slotToRemove.Item = null;
		else
			slotToRemove.ItemCount -= countToRemove;
		return true;
	}
	/// <summary>
	/// Get the Number of one ItemType
	/// </summary>
	/// <param name="itemToFind">The item which you want to find</param>
	/// <returns>the numver of items</returns>
	public ushort GetItemCountFromType(Item itemToFind) {
		ushort sum = 0;
		foreach(UIInventorySlot slot in FindItem(itemToFind))
			sum += slot.ItemCount;
		return sum;
	}
}