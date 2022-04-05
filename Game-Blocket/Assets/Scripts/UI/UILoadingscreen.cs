using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILoadingscreen : MonoBehaviour
{
    public static UILoadingscreen Singleton { get; set; }

    public static bool Visible{ get => Singleton?.gameObject.activeSelf ?? false;
        set{
            if(Singleton != null)
                Singleton.gameObject.SetActive(value);
            else
                Debug.LogWarning("Loadingscreen null!");
        }
    }

    private void Awake() => Singleton = this;
}
