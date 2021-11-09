using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour {

    private readonly List<GameObject> hearthSlots = new List<GameObject>();

    #region HealthSettings
    /// <summary>Full-Heart Sprite</summary>
    public Sprite heart, halfHeart, nullHeart, nullMiddleHeart;
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
            float percent = maxHealth, einzelrange = percent / hearthSlots.Count;
            for (int x = hearthSlots.Count; x > 0; x--)
            {
                float tatsRange = CurrentHealth - (einzelrange * (x - 1));
                if (tatsRange >= einzelrange)
                    Change(heart, x);
                else if (tatsRange >= einzelrange / 2)
                    Change(halfHeart, x);
                else if (tatsRange < einzelrange / 2 && x == hearthSlots.Count)
                    Change(nullHeart, x);
                else
                    Change(nullMiddleHeart, x);
            }
        }
    }
    #endregion

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
            hearthSlots.Add(hc);
        }
    }

    /// <summary>
    /// Changes Grafical view of the current GameObject
    /// </summary>
    /// <param name="sprite">Half-Heart or Heart-sprite</param>
    /// <param name="index">index of the current Heartslot</param>
    private void Change(Sprite sprite,int index)
    {
        GameObject hearthSlot = hearthSlots[index - 1];
        hearthSlot.GetComponent<Image>().sprite = sprite;
        hearthSlot.GetComponent<Image>().color = Color.white;
        if(sprite == null)
            hearthSlot.GetComponent<Image>().color = new Color(255, 255, 255, 0);
    }

}
