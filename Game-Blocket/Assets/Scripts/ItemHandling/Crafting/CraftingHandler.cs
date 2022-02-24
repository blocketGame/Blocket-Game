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
    /// Returns all the Recipes that Contain this specific Item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static List<CraftingRecipe> GetRecipesByItem(byte item)
    {
        //[TODO]
        return new List<CraftingRecipe>();
    }


    /// <summary>
    /// Returns all the Recipes that contain these specific Items (Order Doesn't matter)
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static List<CraftingRecipe> GetRecipesByItems(uint[,] items)
    {
        //[TODO]
        CraftingStation cs =GlobalVariables.ItemAssets.CraftingStations.Find(x => x.CraftingInterfaceSprite.Equals(GlobalVariables.activatedCraftingInterface.GetComponent<Image>().sprite));
        ///Filtering Logic
        Debug.Log(GlobalVariables.ItemAssets.Recipes.FindAll(x => x.Station.Equals(cs)));
        return GlobalVariables.ItemAssets.Recipes.FindAll(x => x.Station.Equals(cs));
    }

    //SORT METHODS INCOMING [TODO]
}
