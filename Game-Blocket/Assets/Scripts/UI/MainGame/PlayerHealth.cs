using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    public static PlayerHealth Singleton { get; private set; }

    #region HealthSettings
    /// <summary>maxHealth of the Player</summary>
    public int maxHealth;
    /// <summary>currentHealth of the Player</summary>
    private float currentHealth;
    /// <summary>Number of HeartContainers</summary>
    [SerializeField]
    private int hearthContainers;
    private List<GameObject> HearthSlots { get; } = new List<GameObject>();

    #region HealthSprites
    public List<HeartLayer> heartLayers = new List<HeartLayer>();

    public Sprite emptyHeartSprite, emptyMiddleHeartSprite;
    #endregion

    public void Awake() {Singleton = this; GameManager.PlayerProfileNow.Health = maxHealth; }

    public float CurrentHealth { get => currentHealth; 
        set{
            if (value < 0)
                UIInventory.Singleton.DeathScreen();
                
            currentHealth = value>100?100:value;
            UIInventory.Singleton.heartStat.text = (int)currentHealth+ "/"+maxHealth;
            float percent = maxHealth==0 ? 100:maxHealth;
            Debug.Log("SETTING VALUE OF HEARTS");
            //Alle hearth layer durchgehen
            for (int h = heartLayers.Count-1; h >= 0; h--)
            {
                float layervalue = (hearthContainers * heartLayers[h].heartValue); //100

                //Prozent eines Heartslots dieses Layers
                //singlevalue = percent / (HearthSlots.Count*heartLayers[h].heartValue); //
                //Durch jeden heartcontainer diesen Layers durchgehen
                for(int x = hearthContainers; x > 0; x--)
                {
                    //Jeden Container checken welchen state dieser hat.
                    if (currentHealth >= ((percent - layervalue) + (heartLayers[h].heartValue * x))) //100-100+10*10 => 100
                    {
                        Change(heartLayers[h].Heart, x);
                        Debug.Log(((percent - layervalue) + (heartLayers[h].heartValue * x)));
                    }
                    else if (currentHealth >= ((percent - layervalue) + heartLayers[h].heartValue * x - heartLayers[h].heartValue / 2)) //100-100+10*10-5 => 95
                        Change(heartLayers[h].halfHeart, x);
                    else if (x == 1)
                        Change(emptyHeartSprite, x);
                    else
                        Change(emptyMiddleHeartSprite, x);
                }
                percent -= layervalue;

            }

            /*
            for (int h = 0; h < heartLayers.Count; h++){
                float percent = maxHealth, singlerange = percent / HearthSlots.Count;
                for (int x = (HearthSlots.Count)- ((HearthSlots.Count / heartLayers.Count) * (h)); x > 0; x--)
                {
                    float tatsRange = CurrentHealth - (singlerange * (x - 1));
                        if (tatsRange >= singlerange)
                            Change(heartLayers[h].Heart, x);
                        else if (tatsRange >= singlerange / 2)
                            Change(heartLayers[h].halfHeart, x);
                        else if (tatsRange < singlerange / 2 && x == (HearthSlots.Count / heartLayers.Count))
                            Change(emptyHeartSprite, x);
                        else
                            Change(emptyMiddleHeartSprite, x);
                }
            }*/
        }
    }
    #endregion
   
    
    /// <summary>
    /// Instantiate HeartContainer GameObject 
    /// </summary>
    public void InitiateSprites(){
        for (int x = 0; x < heartLayers.Count; x++)
            for (int i = 0; i < hearthContainers; i++){
                GameObject hc = new GameObject();
                hc.AddComponent<Image>();
                hc.transform.position = new Vector3(transform.position.x + i ,transform.position.y, transform.position.z);
                hc.transform.localScale = new Vector3(0.01f, 0.01f, 1);
                hc.transform.SetParent(gameObject.transform);
                hc.transform.name = $"HeartSlot{i}";
                HearthSlots.Add(hc);
            }
        
    }

    /// <summary>
    /// Changes Grafical view of the current GameObject
    /// </summary>
    /// <param name="sprite">Half-Heart or Heart-sprite</param>
    /// <param name="index">Index of the current heartslot</param>
    private void Change(Sprite sprite,int index){
        if (!HearthSlots[index - 1].TryGetComponent(out Image image))
            throw new NullReferenceException($"No Image found! Index:{index}");

        image.sprite = sprite;
        image.color = sprite == null ? new Color(255, 255, 255, 0) : Color.white;
    }

}

/// <summary>
/// 
/// </summary>
[Serializable]
public class HeartLayer
{
    /// <summary>
    /// 
    /// </summary>
    public Sprite Heart;
    /// <summary>Half-Heart Sprite</summary>
    public Sprite halfHeart;

    /// <summary>How much health a full heart is</summary>
    public byte heartValue;
}
