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
    [SerializeField]
    private uint station;
    /// <summary>
    /// Crafting Recipe (Representative Items => byte ID)
    /// </summary>
    [SerializeField]
    public byte[] recipe;
    /// <summary>
    /// Item that is created by using this recipe
    /// </summary>
    [SerializeField]
    public Item Output { get; set; }
    #endregion

    #region Properties
    public byte[] Recipe { get => recipe; set => recipe = value; }
    public uint Station { get => station; set => station = value; }
    #endregion

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="station"></param>
    public CraftingRecipe(uint station)
    {
        Recipe = new byte[GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId == station).Slotwidth*GlobalVariables.ItemAssets.CraftingStations.Find(x => x.blockId == station).Slotheight];
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
