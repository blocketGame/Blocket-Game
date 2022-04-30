using System;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TW: Used for MainMenu-Scene<br></br>
/// 
/// Handles the <see cref="Button"/>-Click-Events
/// 
/// </summary>
public class UIMainMenu : MonoBehaviour{
	public static UIMainMenu Singleton { get; private set; }

	/// <summary>
	/// <see cref="Button"/>-Instances from the Scene imported by the Inspector
	/// </summary>
	public Button offlineBtn, onlineBtn, settingsBtn;


	private void Awake() => Singleton = this;

    /// <summary>
    /// Asign <see cref="EventHandler"/> (Listeners) for Button-Presses Event
    /// </summary>
    public void Start() {
		///<see cref="Buttons.offlineBtn"/>
		offlineBtn.onClick.AddListener(() => {
			GlobalVariables.Multiplayer = false;
			SceneManager.LoadScene("Lobby", LoadSceneMode.Additive);
			SceneManager.UnloadSceneAsync("MainMenu");
		});

		///<see cref="Buttons.onlineBtn"/>
		onlineBtn.onClick.AddListener(() => {
			GlobalVariables.Multiplayer = true;
			SceneManager.LoadScene("Lobby", LoadSceneMode.Additive);
			SceneManager.UnloadSceneAsync("MainMenu");
		});

		///<see cref="Buttons.settingsBtn"/>
		settingsBtn.onClick.AddListener(() => { });
		CheckForLoadingScene();
	}

	public static void CheckForLoadingScene(){
		if(SceneManager.sceneCount == 1 && SceneManager.GetSceneAt(0).name != "LoadingScene")
			SceneManager.LoadScene("LoadingScene", new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.None));
	}
}
