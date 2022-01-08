using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    #region HealthSettings
    /// <summary>maxHealth of the Player</summary>
    public float maxHealth;
    /// <summary>currentHealth of the Player</summary>
    private float currentHealth;
    /// <summary>Number of HeartContainers</summary>
    [SerializeField]
    private int hearthContainers;

    public float CurrentHealth { get => currentHealth; 
        set
        {
            currentHealth = value;
            for (int h = 0; h < hearts.Count; h++)
            {
                float percent = maxHealth, einzelrange = percent / hearthSlots.Count;
                for (int x = (hearthSlots.Count )- ((hearthSlots.Count / hearts.Count) * (h)); x > 0; x--)
                {
                    float tatsRange = CurrentHealth - (einzelrange * (x - 1));
                        if (tatsRange >= einzelrange)
                            Change(hearts[h].Heart, x);
                        else if (tatsRange >= einzelrange / 2)
                            Change(hearts[h].half_Heart, x);
                        else if (tatsRange < einzelrange / 2 && x == (hearthSlots.Count / hearts.Count))
                            Change(null_Heart, x);
                        else
                            Change(null_middleHeart, x);
                }
            }
        }
    }
    #endregion

    #region HealthSprites
    public List<HeartLayer> hearts = new List<HeartLayer>();
    public Sprite null_Heart;


    [SerializeField]
    private Sprite null_middleHeart;
    #endregion

    private List<GameObject> hearthSlots= new List<GameObject>();

    /// <summary>
    /// Instantiate HeartContainer GameObject 
    /// </summary>
    public void InitiateSprites()
    {
        for (int h = 0; h < hearts.Count; h++)
        {
            for (int i = 0; i < hearthContainers; i++)
            {
                GameObject hc = new GameObject();
                hc.AddComponent<Image>();
                hc.transform.position = new Vector3(60 + this.transform.position.x + i * 15, -30 + this.transform.position.y, this.transform.position.z);
                hc.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                hc.transform.SetParent(gameObject.transform);
                hc.transform.name = $"HeartSlot{i}";
                hearthSlots.Add(hc);
            }
        }
    }

    /// <summary>
    /// Updates Life in Realtime
    /// </summary>
    private void FixedUpdate()
    {
        
    }

    /// <summary>
    /// Changes Grafical view of the current GameObject
    /// </summary>
    /// <param name="sprite">Half-Heart or Heart-sprite</param>
    /// <param name="index">index of the current Heartslot</param>
    private void Change(Sprite sprite,int index)
    {
        GameObject hearthSlot = hearthSlots[index - 1];
        hearthSlot.gameObject.GetComponent<Image>().sprite = sprite;
        hearthSlot.gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        if(sprite==null)
        hearthSlot.gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
    }

}
[Serializable]
public class HeartLayer
{
    public Sprite Heart;
    /// <summary>Half-Heart Sprite</summary>
    public Sprite half_Heart;
}
