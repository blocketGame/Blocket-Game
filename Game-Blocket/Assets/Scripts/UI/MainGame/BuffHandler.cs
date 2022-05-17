using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffHandler : MonoBehaviour
{
    //Just for test reasons
    //private void Awake() => AddBuffToPlayer(BuffType.Poisened);

    public static BuffHandler Singleton;
    private void Awake() => Singleton = this;

    public void AddBuffToPlayer(BuffType buffType)
    {
        Buff b = ItemAssets.Singleton.Buffs.Find(x => x.buffType == buffType);
        GameObject g = new GameObject(buffType.ToString());
        g.AddComponent<BuffInfliction>().buff = b;
        g.GetComponent<BuffInfliction>().inflictionLength = b.length*10;
        g = GameObject.Instantiate(g, UIInventory.Singleton.buffDisplayingParent.transform);
        g.transform.localScale = Vector3.one/3;
    }
}
