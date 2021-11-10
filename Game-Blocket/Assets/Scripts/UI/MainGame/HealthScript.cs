using System;
using System.Collections;
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
            float percent = maxHealth, einzelrange = percent / HearthSlots.Count;
            for (int x = HearthSlots.Count; x > 0; x--)
            {
                float tatsRange = CurrentHealth - (einzelrange * (x - 1));
                if (tatsRange >= einzelrange)
                    Change(Heart, x);
                else if (tatsRange >= einzelrange / 2)
                    Change(half_Heart, x);
                else if (tatsRange < einzelrange / 2 && x == HearthSlots.Count)
                    Change(null_Heart, x);
                else
                    Change(null_middleHeart, x);
            }
        }
    }
    #endregion

    #region HealthSprites
    /// <summary>Full-Heart Sprite</summary>
    public Sprite Heart;
    /// <summary>Half-Heart Sprite</summary>
    public Sprite half_Heart;

    public Sprite null_Heart;


    [SerializeField]
    private Sprite null_middleHeart;
    #endregion

    private List<GameObject> HearthSlots= new List<GameObject>();

    /// <summary>
    /// Instantiate HeartContainer GameObject 
    /// </summary>
    public void InitiateSprites()
    {
        for (int i = 0; i < hearthContainers; i++)
        {
            GameObject hc = new GameObject();
            hc.AddComponent<Image>();
            hc.transform.position = new Vector3(60 + this.transform.position.x + i * 15, -30 + this.transform.position.y, this.transform.position.z);
            hc.transform.localScale = new Vector3(0.4f, 0.4f, 1);
            hc.transform.SetParent(gameObject.transform);
            hc.transform.name = $"HeartSlot{i}";
            HearthSlots.Add(hc);
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
        GameObject hearthSlot = HearthSlots[index - 1];
        hearthSlot.gameObject.GetComponent<Image>().sprite = sprite;
        hearthSlot.gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        if(sprite==null)
        hearthSlot.gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
    }

}
