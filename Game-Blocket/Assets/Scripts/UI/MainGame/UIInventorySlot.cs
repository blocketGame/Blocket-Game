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
public class UIInventorySlot : MonoBehaviour
{
	/// <summary><see cref="Button"/>-Button</summary>
	public Button button;
	/// <summary><see cref="Image"/>-Button</summary>
	public Image image;
	/// <summary><see cref="Item"/></summary>
	public Item item;

	public void Awake() {
		button.onClick.AddListener(() => {
			//TODO ..
			GlobalVariables.inventory.SwapHand(this);
			if(GlobalVariables.itemSlotButtonPressedLog)
				Debug.Log("Button Pressed");
		});
	}

	

	/// <summary>
	/// Asign <see cref="EventHandler"/> (Listeners) for Button-Presses Event
	/// </summary>
	public void Start() {
		/*
		///<see cref="Buttons.offlineBtn"/>
		offlineBtn.onClick.AddListener(() => {
			SceneManager.LoadScene("MainGame");
		});

		///<see cref="Buttons.onlineBtn"/>
		onlineBtn.onClick.AddListener(() => { });

		///<see cref="Buttons.settingsBtn"/>
		settingsBtn.onClick.AddListener(() => { });*/
	}
}
