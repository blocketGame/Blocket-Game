using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour
{
    [SerializeField] [Range(10,200)]
    public float maxHealth;
    [SerializeField] [Range(0, 200)]
    public float currentHealth;
    
    [SerializeField] 
    private Sprite Heart;
    [SerializeField]
    private Sprite half_Heart;

    [SerializeField]
    private GameObject[] HeartSlots;
    
    void Start()
    {
        maxHealth = 100;
        currentHealth = maxHealth;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (currentHealth > 75)
        {
            HeartSlots[0].gameObject.GetComponent<Image>().sprite = Heart;
            HeartSlots[0].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            HeartSlots[1].gameObject.GetComponent<Image>().sprite = Heart;
            HeartSlots[1].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else if (currentHealth > 50)
        {
            HeartSlots[0].gameObject.GetComponent<Image>().sprite = Heart;
            HeartSlots[0].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            HeartSlots[1].gameObject.GetComponent<Image>().sprite = half_Heart;
            HeartSlots[1].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
        }
        else if (currentHealth > 25)
        {
            HeartSlots[0].gameObject.GetComponent<Image>().sprite = Heart;
            HeartSlots[0].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            HeartSlots[1].gameObject.GetComponent<Image>().sprite = null;
            HeartSlots[1].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
        else if (currentHealth > 0)
        {
            HeartSlots[0].gameObject.GetComponent<Image>().sprite = half_Heart;
            HeartSlots[0].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 255);
            HeartSlots[1].gameObject.GetComponent<Image>().sprite = null;
            HeartSlots[1].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
        else if (currentHealth >= 0)
        {
            HeartSlots[0].gameObject.GetComponent<Image>().sprite = null;
            HeartSlots[0].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
            HeartSlots[1].gameObject.GetComponent<Image>().sprite = null;
            HeartSlots[1].gameObject.GetComponent<Image>().color = new Color(255, 255, 255, 0);
        }
    }
}
