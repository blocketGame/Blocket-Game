using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BuffInfliction : MonoBehaviour
{
    public Buff buff; //for specifying in Assets
    public int multiplyer; //how often has the buff been stacked
    public float inflictionLength; //how long does the buff last


    private void Awake() => gameObject.AddComponent<Image>();

    void Update()
    {
        if (gameObject.GetComponent<Image>().sprite == null)
            gameObject.GetComponent<Image>().sprite = buff.buffPicture;

        if (inflictionLength<=0)
            GameObject.Destroy(gameObject);

        //Inflict Buff
        switch (buff.buffType)
        {
            case BuffType.Poisened:
                //Apply Poison
                PlayerHealth.Singleton.CurrentHealth -= 1 * Time.deltaTime;
                break;
        }

        inflictionLength -= Time.deltaTime;
    }
}
[Serializable]
public enum BuffType
{
    Poisened,
    Gods_Blessing,
    Defense,
    Regeneration,
    Confused,
    Trauma
}
[Serializable]
public class Buff
{
    public Sprite buffPicture;
    public BuffType buffType;
    public int length;
}