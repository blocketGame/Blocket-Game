using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour{
    public static PlayerHealth Singleton { get; private set; }

    #region HealthSettings
    /// <summary>maxHealth of the Player</summary>
    public ushort maxHealth;
    /// <summary>currentHealth of the Player</summary>
    private ushort currentHealth;
    /// <summary>Number of HeartContainers</summary>
    [SerializeField]
    private int hearthContainers;
    private List<GameObject> HearthSlots { get; } = new List<GameObject>();

    #region HealthSprites
    public List<HeartLayer> heartLayers = new List<HeartLayer>();

    public Sprite emptyHeartSprite, emptyMiddleHeartSprite;
    #endregion

    public void Awake() => Singleton = this;

    public ushort CurrentHealth { get => currentHealth; 
        set{
            currentHealth = value;
            for (int h = 0; h < heartLayers.Count; h++){
                float percent = maxHealth, singlerange = percent / HearthSlots.Count;
                for (int x = (HearthSlots.Count )- ((HearthSlots.Count / heartLayers.Count) * (h)); x > 0; x--)
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
            }
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
