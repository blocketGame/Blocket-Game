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
			else{ 
				Debug.LogWarning("Loadingscreen null!");
				UIMainMenu.CheckForLoadingScene();
			}
		}
	}

	private void Awake() { 
		Singleton = this;
		Visible = GameManager.State == GameState.LOADING;
    }
}
