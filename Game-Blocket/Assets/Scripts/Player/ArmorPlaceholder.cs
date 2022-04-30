using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small Script for setting Images of Armor outside of the Player
/// </summary>
public class ArmorPlaceholder : MonoBehaviour
{
    public static ArmorPlaceholder Singleton { get; private set; }

    public GameObject CharacterPreview;
    [Tooltip("0 => Helmet \n 1 => ChestPlate \n 2 => Leggins")]
    public List<SpriteRenderer> ArmorRenderer; 

    public void SetArmorSprite(int armorId,Sprite sprite) => ArmorRenderer[armorId].sprite = sprite;
    private void Instantiate()
    {
        Singleton = this;
        ClearArmor();
    }
    public void ClearArmor()
    {
        foreach (SpriteRenderer sr in ArmorRenderer) sr.sprite = null;
    }

    void Start() => GameObject.Destroy(CharacterPreview);
    private void Awake() => Instantiate();
}
