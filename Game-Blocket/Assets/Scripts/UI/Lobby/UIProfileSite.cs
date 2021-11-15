using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProfileSite : MonoBehaviour
{
	[Header("Static Resources")]
	public GameObject worldSelectionSite;
	public GameObject characterSelectionSite;
	public Button backBtn, selectBtn, characterSlectBtn, worldSelectBtn;

	private bool _characterSelectonOpen = false;
	public bool CharacterSelectonOpen { get { return _characterSelectonOpen; } set { 
			_characterSelectonOpen = value;
			characterSelectionSite.SetActive(value);
			worldSelectionSite.SetActive(!value);
		} 
	}

	private void InitButtons(){
		backBtn.onClick.AddListener(() => { 
		
		});
		selectBtn.onClick.AddListener(() => {

		});
		characterSlectBtn.onClick.AddListener(() => {

		});
		worldSelectBtn.onClick.AddListener(() => {

		});
    }

	public void Awake()
	{
		GlobalVariables.UIProfileSite = this;
		InitButtons();
	}
}
