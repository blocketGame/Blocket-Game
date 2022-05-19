using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
	public static PauseMenu Singleton { get; private set; }

	public GameObject pauseMenuSide;

	public Button continueBtn, closeGameBtn, backToMainMenuBtn;

	public static bool PauseMenuOpen { get => Singleton.pauseMenuSide.activeInHierarchy; set {
			Singleton.pauseMenuSide.SetActive(value);
			GameManager.State = value ? GameState.PAUSED : GameState.LOADING;//TODO ingame or loading?
			if(value){//Close other
				UIInventory.Singleton.InventoryOpened = false;
            }
		}
	}

	private void Update() {
		if(Input.GetKeyDown(KeyCode.Escape) && !UIInventory.Singleton.ChatOpened && !UIInventory.Singleton.deathScreen.activeSelf)
			PauseMenuOpen = !PauseMenuOpen;
	}

	private void Start() => PauseMenuOpen = false;

	private void Awake() {
		Singleton = this;
		continueBtn.onClick.AddListener(() => {
			PauseMenuOpen = false;
		});
		closeGameBtn.onClick.AddListener(() => {
			Application.Quit(0);
		});
		backToMainMenuBtn.onClick.AddListener(() => {
			//TODO: Save
			UILobby.BackToMainMenuAct();
		});
	}
}
