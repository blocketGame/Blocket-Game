using MLAPI;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProfileSite : MonoBehaviour {
	[Header("Static Resources")]
	public Text createInput;
	public GameObject worldSelectionSite, characterSelectionSite, listContentPrefab;
	public Button backBtn, nextBtn, characterSlectBtn, worldSelectBtn, createBtn;
	public ScrollRect playerScrollRect, worldScrollRect;

	private RectTransform _playerContent, _worldContent;

	private bool _characterSelectionOpen;
	private bool CharacterSelectionOpen { get => _characterSelectionOpen; set {
			_characterSelectionOpen = value;
			characterSelectionSite.SetActive(value);
			worldSelectionSite.SetActive(!value);
		}
	}

	private List<string> FoundPlayerProfiles { get => _foundCharacterProfiles; set {
			_foundCharacterProfiles = value;
			foreach (string profile in value) {
				ListContentUI uiPSC = Instantiate(listContentPrefab, _playerContent.transform).GetComponent<ListContentUI>();
				int x = profile.LastIndexOf(@"\"), y = profile.LastIndexOf('.');
				uiPSC.contentName.text = profile.Substring(x+1, y-x-1);
			}}}
	private List<string> _foundCharacterProfiles = new List<string>();

	private List<string> FoundWorldProfiles { get => _foundWorldProfiles; set { 
			_foundWorldProfiles = value;
			foreach(string profile in value) {
				ListContentUI uiPSC = Instantiate(listContentPrefab, _worldContent.transform).GetComponent<ListContentUI>();
				uiPSC.contentName.text = profile;
			}}} 
	private List<string> _foundWorldProfiles= new List<string>();

	private bool _characterSelectonOpen;
	public bool CharacterSelectonOpen { get { return _characterSelectonOpen; } set { 
			_characterSelectonOpen = value;
			characterSelectionSite.SetActive(value);
			worldSelectionSite.SetActive(!value);
		} 
	}

	public void SelectItem() {
		if (CharacterSelectionOpen && NetworkManager.Singleton.IsClient)
				CharacterSelectionOpen = false;
			else
				if(((GameManager.playerProfileNow == null) != NetworkManager.Singleton.IsClient) && 
				((GameManager.worldProfileNow == null) != NetworkManager.Singleton.IsServer))
					GlobalVariables.UILobby.SiteIndexOpen = 1;
	}

	private void InitButtons(){
		if(GlobalVariables.muliplayer)
			if(NetworkManager.Singleton.IsClient)
				worldSelectBtn.interactable = false;

		backBtn.onClick.AddListener(() => {
			GlobalVariables.UILobby.SiteIndexOpen = 0;
		});
		
		nextBtn.onClick.AddListener(SelectItem);

		characterSlectBtn.onClick.AddListener(() => {
			CharacterSelectionOpen = true;
		});
		worldSelectBtn.onClick.AddListener(() => {
			CharacterSelectionOpen = false;
		});
		createBtn.onClick.AddListener(() => {
			if (!ValidateInput())
				return;
			if (CharacterSelectionOpen) { 
				PlayerProfile p = new PlayerProfile(createInput.text, null);
				FileHandler.ExportProfile(p, true);
				GameManager.playerProfileNow = p;
			} else {
				WorldProfile p = new WorldProfile(createInput.text, null);
				FileHandler.ExportProfile(p, false);
				GameManager.worldProfileNow = p;
			}
			FindAllprofiles();
			SelectItem();
			///TODO: Characterdialoge...

		});
	}

	public bool ValidateInput() {
		if(createInput.text == null || createInput.text.Trim() == "")
			return false;
		return true;
	}

	public void Start() {
		FindAllprofiles();
	}

	public void FindAllprofiles() {
		FoundPlayerProfiles = FileHandler.FindAllPlayerProfiles();
		Debug.Log(FoundPlayerProfiles.Count);
	}

	public void Awake()
	{
		CharacterSelectionOpen = true;
		GlobalVariables.UIProfileSite = this;
		InitButtons();
		_playerContent = playerScrollRect.content;
		_worldContent = worldScrollRect.content;
	}
}
