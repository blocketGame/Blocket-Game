using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathScreen : MonoBehaviour
{

    public void Respawn()
    {
        UIInventory.Singleton.RespawnUI();
        PlayerHealth.Singleton.CurrentHealth = PlayerHealth.Singleton.maxHealth;
        GameManager.SwitchDimension(Dimension.OVERWORLD);
        GlobalVariables.LocalPlayer.transform.position = new Vector3(0, 10);
    }
}
