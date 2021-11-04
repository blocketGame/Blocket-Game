using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    #region HealthSettings
    [SerializeField] [Range(10,200)]
    public float maxHealth;
    [SerializeField]
    [Range(0, 200)]
    private float currentHealth;
    [SerializeField]
    private int hearthContainers;
    #endregion

    #region HealthSprites
    [SerializeField] 
    private Sprite Heart;
    [SerializeField]
    private Sprite half_Heart;
    #endregion

    private List<GameObject> HearthSlots= new List<GameObject>();

    public float CurrentHealth { get => currentHealth; set => currentHealth = value; }

    void Start()
    {
        maxHealth = 100;
        CurrentHealth = maxHealth;
    }

    private void Awake()
    {
        for(int i = 0; i < hearthContainers; i++)
        {
            GameObject hc = Instantiate(new GameObject(),this.transform);
            hc.AddComponent<Image>();
            hc.transform.position = new Vector3(60+this.transform.position.x+i * 60,-30+ this.transform.position.y, this.transform.position.z);
            hc.transform.localScale = new Vector3(0.5f,0.5f,1);
            hc.transform.parent = this.gameObject.transform;
            HearthSlots.Add(hc);
        }
    }

    private void FixedUpdate()
    {
        float percent = 100;
        float einzelrange = percent / HearthSlots.Count;
        for (int x = HearthSlots.Count;x>0;x-- )
        {
            ///[TODO]
            float tatsRange = CurrentHealth - (einzelrange * (x-1));
            if (tatsRange >= einzelrange)
                Change(Heart, x);
            else if (tatsRange >= einzelrange / 2)
                Change(half_Heart, x);
            else
                Change(null, x); 
        }
    }

    private void Change(Sprite sprite,int index)
    {
        GameObject hearthSlot = HearthSlots[index - 1];
        hearthSlot.gameObject.GetComponent<Image>().sprite = sprite;
        hearthSlot.gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        if(sprite==null)
        hearthSlot.gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);

    }
}
