using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles and Sorts the Recipes that are located in the assets
/// Doesn't need a State (static) , because it is just meant as a bridge between Recipes and Crafting System 
/// => Calculation
/// </summary>
public static class CraftingHandler 
{
    
    /// <summary>
    /// Returns all the Recipes that contain these specific Items (Order Doesn't matter)
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<CraftingRecipe> GetRecipesByItems(Craftable[] items)
    {
        //[TODO]
        CraftingStation cs = ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(GlobalVariables.ActivatedCraftingInterface?.GetComponent<Image>()?.sprite ?? UIInventory.Singleton.handCrafting.GetComponent<Image>().sprite));
        Debug.Log(cs.blockId);
        ///Filtering Logic
        foreach (CraftingRecipe cr in ItemAssets.Singleton.Recipes.FindAll(x => x.Station.Equals(cs.blockId)))
        {
            int stelle=0; 
            foreach (Craftable i in cr.Recipe)
            {
                if (items[stelle].Item?.id == i.Item?.id && i.Item?.id != 0 && i.Item!=null)
                {
                    Debug.Log("Recommendation Should be created!!");
                    yield return cr;
                }
                
                stelle++;
            }
        } 
    }

    /// <summary>
    /// Returns all the Recipes that Contain this specific Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Craftable GetExactItem(Craftable[] items, out CraftingRecipe usedCraftingRecipe)
    {
        //[TODO]
        CraftingStation cs = ItemAssets.Singleton.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(GlobalVariables.ActivatedCraftingInterface?.GetComponent<Image>()?.sprite ?? UIInventory.Singleton.handCrafting.GetComponent<Image>().sprite));
        ///Filtering Logic
        foreach (CraftingRecipe cr in ItemAssets.Singleton.Recipes.FindAll(x => x.Station.Equals(cs.blockId)))
        {
            int stelle = 0;
            bool correct=true;
            foreach (Craftable i in cr.Recipe)
            {
                if ((items[stelle].ItemID != i.ItemID || items[stelle].count < i.count) && i.ItemID != 0)
                {
                    correct=false;
                }

                stelle++;
            }
            if (correct)
            {
                usedCraftingRecipe = cr;
                return cr.Output;
            }
        }
        usedCraftingRecipe = null;
        return new Craftable();

    }

    //SORT METHODS INCOMING [TODO]
}
