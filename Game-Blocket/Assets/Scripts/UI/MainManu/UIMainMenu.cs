using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TW: Used for MainMenu-Scene<br></br>
/// 
/// Handles the <see cref="Button"/>-Click-Events
/// 
/// </summary>
public class UIMainMenu : MonoBehaviour
{

	/// <summary>
	/// <see cref="Button"/>-Instances from the Scene imported by the Inspector
	/// </summary>
	public Button offlineBtn, onlineBtn, settingsBtn;

	/// <summary>
	/// Asign <see cref="EventHandler"/> (Listeners) for Button-Presses Event
	/// </summary>
	public void Start() {
		///<see cref="Buttons.offlineBtn"/>
		offlineBtn.onClick.AddListener(() => {
			SceneManager.LoadScene("MainGame");
			GlobalVariables.muliplayer = false;
		});

		///<see cref="Buttons.onlineBtn"/>
		onlineBtn.onClick.AddListener(() => {
			SceneManager.LoadScene("Lobby");
			SceneManager.UnloadSceneAsync("MainMenu");
			GlobalVariables.muliplayer = true;
		});

		///<see cref="Buttons.settingsBtn"/>
		settingsBtn.onClick.AddListener(() => { });
	}
}
