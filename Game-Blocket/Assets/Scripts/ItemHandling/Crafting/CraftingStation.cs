
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

    [SerializeField]
    [Tooltip("Refered Crafting Station Block")]
    public byte blockId;

    #region Slot Specifications
    [Header("Slot Specifications")]
    /// <summary>
    /// Slot Size 
    /// </summary>
    [SerializeField][Tooltip("Horizontal Slot Count")][Range(1,10)]
    private byte slotwidth;
    [SerializeField][Tooltip("Vertical Slot Count")][Range(1, 10)]
    private byte slotheight;
    #endregion

    #region Sprite Settings
    [Header("Sprite Settings")]
    /// <summary>
    /// Interface background Sprite [TODO : Dynamic instantiating of the Crafting if]
    /// </summary>
    [SerializeField][Tooltip("Background of the Crafting Interface")]
    private Sprite craftingInterfaceSprite;
    [SerializeField][Tooltip("Background for the Crafting Interface Slots")]
    private Sprite craftingSlotSprite;
    #endregion

    #region Color Specifications
    [Header("Color Specifications")]

    [Tooltip("Color Specification for the Listview Element")]
    public Color listViewColor;
    [Tooltip("Color Specification for the Crafting Button Element")]
    public Color buttonColor;
    #endregion


    public bool activatedCraftingInterface { get; set; }
    public GameObject outBtn { get; set; }

    #region Properties
    public byte Slotwidth { get => slotwidth; set => slotwidth = value; }
    public byte Slotheight { get => slotheight; set => slotheight = value; }
    public Sprite CraftingInterfaceSprite { get => craftingInterfaceSprite; set => craftingInterfaceSprite = value; }

    private static List<BlockItem> Blockitems { get => GlobalVariables.ItemAssets.BlockItemsInGame; }
    private static List<ToolItem> ToolItems { get => GlobalVariables.ItemAssets.ToolItemsInGame; }

    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="blockHoverdAbsolute"></param>
    /// <param name="ctStation"></param>
    /// <returns></returns>
    public static CraftingStation HandleCraftingInterface(Vector2Int blockHoverdAbsolute, CraftingStation ctStation)
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
    /// Instantiates the crafting interface with all custom parameters
    /// </summary>
    /// <param name="prefabItemSlot"></param>
    /// <param name="CraftingInterfacePlaceholder"></param>
    /// <param name="craftingStation"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public static void InstatiateCraftingInterface(GameObject prefabItemSlot, GameObject CraftingInterfacePlaceholder, CraftingStation craftingStation, byte width, byte height)
    {
        PrepareButton(prefabItemSlot, CraftingInterfacePlaceholder, craftingStation);
        InstatiateSlots(prefabItemSlot, CraftingInterfacePlaceholder, craftingStation, width, height, 0, 0);
        CustomColourization(CraftingInterfacePlaceholder, craftingStation);
    }

    /// <summary>
    /// Prepares the button by setting the onclick event
    /// </summary>
    /// <param name="prefabItemSlot"></param>
    /// <param name="CraftingInterfacePlaceholder"></param>
    /// <param name="craftingStation"></param>
    private static void PrepareButton(GameObject prefabItemSlot, GameObject CraftingInterfacePlaceholder, CraftingStation craftingStation)
    {
        craftingStation.outBtn = CraftingInterfacePlaceholder.GetComponentInChildren<Button>().gameObject;
        craftingStation.outBtn.GetComponent<Button>().onClick.AddListener(() => CraftEvent(CraftingInterfacePlaceholder, craftingStation));
    }

    /// <summary>
    /// Generate Slots in the crafting interface
    /// (Happens to be just functional for the first crafting tables [must be updated at times])
    /// </summary>
    /// <param name="prefabItemSlot"></param>
    /// <param name="CraftingInterfacePlaceholder"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private static void InstatiateSlots(GameObject prefabItemSlot, GameObject CraftingInterfacePlaceholder, CraftingStation craftingStation, byte width, byte height, int i, int o)
    {
        if (i < width)
            if (o < height)
            {
                GameObject s = GameManager.Instantiate(prefabItemSlot, CraftingInterfacePlaceholder.transform);
                s.GetComponent<UIInventorySlot>().button.gameObject.GetComponent<Image>().sprite = craftingStation.craftingSlotSprite;
                s.GetComponent<UIInventorySlot>().CraftingStation = craftingStation;
                s.GetComponent <UIInventorySlot>().parentCraftingInterface = CraftingInterfacePlaceholder;
                s.transform.position = new Vector3(s.transform.position.x + 90 + i * 70, s.transform.position.y - 130 - o * 70, s.transform.position.z);
                InstatiateSlots(prefabItemSlot, CraftingInterfacePlaceholder, craftingStation, width, height, i, o + 1);
            }
            else InstatiateSlots(prefabItemSlot, CraftingInterfacePlaceholder, craftingStation, width, height, i + 1, 0);
    }

    /// <summary>
    /// Colours the View & Btns in the right way
    /// </summary>
    /// <param name="CraftingInterfacePlaceholder"></param>
    /// <param name="craftingStation"></param>
    private static void CustomColourization(GameObject CraftingInterfacePlaceholder, CraftingStation craftingStation)
    {
        CraftingInterfacePlaceholder.GetComponentInChildren<ScrollRect>().gameObject.GetComponent<Image>().color = craftingStation.listViewColor;
        CraftingInterfacePlaceholder.GetComponentInChildren<Button>().GetComponent<Image>().color = craftingStation.buttonColor;
        
    }

    /// <summary>
    /// Button Event triggers this crafting event
    /// </summary>
    /// <param name="CraftingInterfacePlaceholder"></param>
    /// <param name="craftingStation"></param>
    private static void CraftEvent(GameObject CraftingInterfacePlaceholder, CraftingStation craftingStation)
    {
        int x = 0;
        Craftable[] array = new Craftable[craftingStation.Slotwidth * craftingStation.Slotheight];
        foreach (UIInventorySlot uislot in CraftingInterfacePlaceholder.GetComponentsInChildren<UIInventorySlot>())
        {
            array[x] = new Craftable(uislot.ItemID, uislot.ItemCount); x++;
        }
        Craftable i = CraftingHandler.GetExactItem(array,out CraftingRecipe craftingRecipe);
        if (i.Item.id != 0)
        {
            GlobalVariables.Inventory.AddItem(i.Item.id, i.count, out ushort item);
            foreach(UIInventorySlot uIInventorySlot in CraftingInterfacePlaceholder.GetComponentsInChildren<UIInventorySlot>())
            {
                if(uIInventorySlot.ItemCount!=0)
                    foreach(Craftable c in craftingRecipe.Recipe)
                        if(c.Item.id.Equals(uIInventorySlot.ItemID))
                            uIInventorySlot.ItemCount-= c.count;
                if(uIInventorySlot.ItemCount == 0)
                {
                    uIInventorySlot.ItemID = 0;
                    uIInventorySlot.itemImage = null;
                }
            }
        }
        else Debug.LogError("Naughty Naughty , you can't craft something that doesn't exist!");

        ///[TODO - Remove Items that have been used to craft the new one]
    }

    /// <summary>
    /// Reacts to the ItemInsertEvent -> Renews crafting Recommendations
    /// </summary>
    public static void RenewRecommendations(Craftable[] items, GameObject CraftingInterfacePlaceholder)
    {
        IEnumerable<CraftingRecipe> recipes = CraftingHandler.GetRecipesByItems(items); //RecipeResponse
        CraftingInterfacePlaceholder.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Mask>().GetComponentInChildren<RecipeRecommendationList>().PruneRecommendations();

        Item i = CraftingHandler.GetExactItem(items,out CraftingRecipe c).Item;
        if (i != null)
        {
            CraftingInterfacePlaceholder.GetComponentInChildren<CraftingButtonRecognition>().itemName.text = i.name;
            CraftingInterfacePlaceholder.GetComponentInChildren<CraftingButtonRecognition>().itemImage.sprite = i.itemImage;
        }else
        {
            CraftingInterfacePlaceholder.GetComponentInChildren<CraftingButtonRecognition>().itemName.text = "Click me to craft!";
            CraftingInterfacePlaceholder.GetComponentInChildren<CraftingButtonRecognition>().itemImage.sprite = null;
        }

        int offSetY=0;
        ///Insert into Graphical View
        foreach (CraftingRecipe cr in recipes)
        {
            GameObject g = GameObject.Instantiate(GlobalVariables.PrefabAssets.craftingUIListView, CraftingInterfacePlaceholder.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Mask>().GetComponentInChildren<RecipeRecommendationList>().transform);
            CraftingInterfacePlaceholder.GetComponentInChildren<ScrollRect>().GetComponentInChildren<Mask>().GetComponentInChildren<RecipeRecommendationList>().currentRecommendations.Add(g);
            g.transform.localPosition.Set(g.transform.localPosition.x, g.transform.localPosition.y + offSetY, g.transform.localPosition.z);
            RecipeShowCase rcs = g.GetComponentInChildren<RecipeShowCase>();
            rcs.Name.text = cr.Output.Item.name.Length < 10? cr.Output.Item.name: cr.Output.Item.name.Substring(0,9);
            rcs.Description.text = cr.Output.Item.description;
            rcs.imagePreview.sprite = cr.Output.Item.itemImage;
            offSetY += 100;
        }
        ///Spawning Recommendations
        Debug.Log(GlobalVariables.activatedCraftingInterface.name);
        }
}
