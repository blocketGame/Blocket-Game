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
        GetComponent<Image>().sprite = PickARandomNumber();
    }

    public Sprite PickARandomNumber(){
            Sprite image = null;
            switch (Random.Range(0, 3))
            {
                case 0:
                    if(itemAssets.BlockItemsInGame.Count == 0)
                        goto case 1;
                    Item i = itemAssets.BlockItemsInGame[Random.Range(0, itemAssets.BlockItemsInGame.Count)];
                    image = i?.itemImage;
                    ItemName.text = i?.name;
                    break;
                case 1:
                    if(itemAssets.WeaponItems.Count == 0)
                        goto case 2;
                    Item it = itemAssets.WeaponItems[Random.Range(0, itemAssets.UseableItemsInGame.Count)];
                    image = it?.itemImage;
                    ItemName.text = it?.name; 
                    break;
                case 2:
                    if(itemAssets.ProjectileItems.Count == 0)
                        break;//@Philipp pls
                    Item pr = itemAssets.ProjectileItems[Random.Range(0, itemAssets.ProjectileItems.Count)];
                    image = pr?.itemImage;
                    ItemName.text = pr?.name;
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
