
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crafting station Class 
/// </summary>
[System.Serializable]
public class CraftingStation 
{
    /// <summary>
    /// Slot Size 
    /// </summary>
    [SerializeField]
    private byte slotwidth;
    [SerializeField]
    private byte slotheight;
    /// <summary>
    /// Interface background Sprite [TODO : Dynamic instantiating of the Crafting if]
    /// </summary>
    [SerializeField]
    private Sprite craftingInterfaceSprite;
    [SerializeField]
    public byte blockId;
    public bool activatedCraftingInterface;

    #region Properties
    public byte Slotwidth { get => slotwidth; set => slotwidth = value; }
    public byte Slotheight { get => slotheight; set => slotheight = value; }
    public Sprite CraftingInterfaceSprite { get => craftingInterfaceSprite; set => craftingInterfaceSprite = value; }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="blockHoverdAbsolute"></param>
    /// <param name="ctStation"></param>
    /// <returns></returns>
    public static CraftingStation HandleCraftingInterface(Vector2Int blockHoverdAbsolute,CraftingStation ctStation)
    {
        if (GlobalVariables.activatedCraftingInterface is null)
        {
            GameObject gc = new GameObject("Crafting Interface");
            gc.transform.position = new Vector3(blockHoverdAbsolute.x + 0.5f, blockHoverdAbsolute.y + 6.5f, 0);
            gc.AddComponent<RectTransform>();
            gc.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 8);
            gc.AddComponent<Canvas>();
            gc.AddComponent<Image>();
            gc.GetComponent<Image>().sprite = ctStation.CraftingInterfaceSprite;
            GlobalVariables.activatedCraftingInterface = gc;
        }
        else if (GlobalVariables.activatedCraftingInterface.activeSelf)
        {
            GameManager.Destroy(GlobalVariables.activatedCraftingInterface);
            GlobalVariables.activatedCraftingInterface = null;
        }
        return ctStation;
    }

    /// <summary>
    /// Generate Slots in the crafting interface
    /// (Happens to be just functional for the first crafting table [must be updated at times])
    /// </summary>
    /// <param name="prefabItemSlot"></param>
    /// <param name="CraftingInterfacePlaceholder"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void InstatiateSlots(GameObject prefabItemSlot, GameObject CraftingInterfacePlaceholder,CraftingStation craftingStation,byte width,byte height)
    {
        for (int i = 0; i < width; i++)
        {
            for (int o = 0; o < height; o++)
            {
                GameObject s = GameManager.Instantiate(prefabItemSlot, CraftingInterfacePlaceholder.transform);
                s.GetComponent<UIInventorySlot>().CraftingStation = craftingStation;
                s.transform.position = new Vector3(s.transform.position.x + 90+i* 70, s.transform.position.y - 130-o*70, s.transform.position.z);
            }
        }
    }

    /// <summary>
    /// Reacts to the ItemInsertEvent -> Renews crafting Recommendations
    /// </summary>
    public void RenewRecommendations(uint[,] items)
    {
        //[TODO]
        ///=> RecipeRequest (CraftingHandler)
        ///<= RecipeResponse
        ///
        //GlobalVariables.activatedCraftingInterface.GetComponentInChildren<ListContentUI>().

        ///RecipeRequest
        CraftingHandler.GetRecipesByItems(items); //RecipeResponse

        ///Insert into Graphical View
    }
}
