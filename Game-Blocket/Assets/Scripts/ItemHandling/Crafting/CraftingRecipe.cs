using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logical Recipe for making Items in the Game 
/// (contains every logical reference to it's Item components as well as the crafting station)
/// </summary>

[CreateAssetMenu]
public class CraftingRecipe : ScriptableObject, ISerializationCallbackReceiver
{
    #region Logical References
    /// <summary>
    /// Crafting station entity , needed for specification were it can be crafted
    /// </summary>
    [SerializeField][Tooltip("Refers to the saved items in @ItemAssets")]
    private uint station;
    /// <summary>
    /// Each count refers to the Item of the same Id
    /// </summary>
    [SerializeField][Tooltip("Items that are needed for crafting this Recipe (Needs to be in the right order to be recognized by the system)")]
    public Craftable[] recipe;
    public uint GetCountOfItem(uint itemId) => recipe[itemId].count;

    /// <summary>
    /// Item that is created by using this recipe
    /// </summary>
    [SerializeField][Tooltip("Crafting this recipe results in that item")]
    public uint output;
    #endregion

    #region Properties
    public Craftable[] Recipe { get => recipe; set => recipe = value; }
    public uint Station { get => station; set => station = value; }
    public Item Output { get => GlobalVariables.ItemAssets.GetItemFromItemID(output); }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="station"></param>
    public CraftingRecipe(uint station)
    {
        Recipe = new Craftable[GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId == station).Slotwidth*GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId == station).Slotheight];
    }

    /// <summary>
    /// Compare to the recipe Array and return the object that will be crafted 
    /// </summary>
    /// <param name="recipe"></param>
    /// <returns></returns>
    public CraftingRecipe craftingRecipe(byte[,] recipe)
    {
        if (recipe.Equals(recipe))
        {
            return this;
        }
        return null;
    }


    #region ScriptableObjectMethods
    public void OnBeforeSerialize()
    {
        //throw new System.NotImplementedException();
    }

    public void OnAfterDeserialize()
    {
        //throw new System.NotImplementedException();
    }
    #endregion
}
[Serializable]
public struct Craftable
{
    public uint item;
    public uint count;

}
