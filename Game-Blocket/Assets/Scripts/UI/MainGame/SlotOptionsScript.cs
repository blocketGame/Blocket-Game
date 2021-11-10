using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotOptionsScript : MonoBehaviour, IPointerClickHandler {

    #region StoredObjects
    public UIInventorySlot invSlot;
    public GameObject SlotOptions;
    #endregion

    /// <summary>
    /// React on any Click Event from a UIInvSlot
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        ///React on LeftClick Event
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject.FindWithTag("Player").GetComponent<Inventory>().PressedSlot(invSlot);
            if (GlobalVariables.itemSlotButtonPressedLog)
                Debug.Log("Button Pressed");

            SlotOptions = GameObject.FindWithTag("SlotOptions");
            if (SlotOptions == null)
                return;

            Vector3 vector3 = new Vector3(-1000,-1000,-50);
            SlotOptions.transform.position = vector3;

        }
        ///React on MiddleClick Event
        else if (eventData.button == PointerEventData.InputButton.Middle)
        {
            Debug.Log("Middle click");
            ///[TODO]
        }
        ///React on RightClick Event
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            SlotOptions = GameObject.FindWithTag("SlotOptions");
            if (SlotOptions == null)
                return;
            SlotOptions.transform.position = Input.mousePosition;
            ///ContentActions :
            ///[NOT IMPLEMENTED]
            ///[TODO]
        }
    }


}
