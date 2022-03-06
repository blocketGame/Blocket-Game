using UnityEngine;
using UnityEngine.UI;

public class ListContentUI : MonoBehaviour
{
	public Text contentName;
	public Button mainBtn, deleteBtn, renameBtn;

	public static string selectedBtnNameCharacter, selectedBtnNameWorld;

	public bool CharacterBtn { get; set; }

	/// <summary>
	/// UNDONE
	/// </summary>
	private static Button _btnSelectedNow;
	public static Button BtnSelectedNow { get => _btnSelectedNow;
		 set {
			_btnSelectedNow = value;
		} 
	}

	public void Awake() {
		mainBtn.onClick.AddListener(() => {
			if(CharacterBtn)
				selectedBtnNameCharacter = contentName.text;
			else
				selectedBtnNameWorld = contentName.text;
			GlobalVariables.UIProfileSite.SelectedItem();
		});

		deleteBtn.onClick.AddListener(() => {
			if(DebugVariables.ShowlobbyButtons)
				Debug.Log("Delete");
		});
		renameBtn.onClick.AddListener(() => {
			if (DebugVariables.ShowlobbyButtons)
				Debug.Log("Rename");
		});
	}
}
