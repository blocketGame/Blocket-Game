using System;
using System.Collections.Generic;

public class Inventory {

	public List<UIInventorySlot> armorSlots, accessoiresSlots, invSlots;
	public UIInventorySlot atHand;

	public bool SwapHand(UIInventorySlot slotPressed) {
		return false;
	}
}