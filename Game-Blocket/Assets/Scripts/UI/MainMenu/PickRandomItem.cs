using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickRandomItem : MonoBehaviour
{
    public ItemAssets itemAssets;
    public Text ItemName => GetComponentInChildren<Text>();
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Image>().sprite = PickARandomNumber();
    }

    public Sprite PickARandomNumber()
    {
            Sprite image = null;
            switch (Random.Range(0, 2))
            {
                case 0:
                    Item i = itemAssets.BlockItemsInGame[Random.Range(0, itemAssets.BlockItemsInGame.Count)];
                    image = i?.itemImage;
                    ItemName.text = i?.name;
                    break;
                case 1:
                    Item it = itemAssets.UseableItemsInGame[Random.Range(0, itemAssets.UseableItemsInGame.Count)];
                    image = it?.itemImage;
                    ItemName.text = it?.name; 
                    break;
                    /*
                case 2:
                    image = GlobalVariables.ItemAssets.ToolItemsInGame[Random.Range(0, GlobalVariables.ItemAssets.CommonItems.Count)]?.itemImage; break;
                case 3:
                    image = GlobalVariables.ItemAssets.CommonItems[Random.Range(0, GlobalVariables.ItemAssets.CommonItems.Count)]?.itemImage; break;
                default:
                    image = GlobalVariables.ItemAssets.EquipableItemsInGame[Random.Range(0, GlobalVariables.ItemAssets.CommonItems.Count)]?.itemImage; break;
                    */
            }
            if (image != null)
                return image;
            else
                return PickARandomNumber();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
