using System.Collections;

using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Class handles the visible GO
/// </summary>
public class ConsoleEntry : MonoBehaviour{
    public float waitBeforeVanishVisible = 1.5f;

    /// <summary>Gameobject which will be Instantiatet in the Chat</summary>
    public GameObject ChatGO { get; set; }

    /// <summary>Text Component</summary>
    public Text text;

    Coroutine VanishCoroutine { get; set; }

    public void Start()
    {
        if(gameObject.transform.parent != UIInventory.Singleton.chatParent.parent){
            return;
        }
        VanishCoroutine = StartCoroutine(nameof(Vanish));
        ChatGO = Instantiate(gameObject, UIInventory.Singleton.chatParent);
        UIInventory.Singleton.chatHistoryView.Add(ChatGO.GetComponent<RectTransform>());
    }

    public IEnumerator Vanish(){
        yield return new WaitForSeconds(waitBeforeVanishVisible);
        Destroy(ConsoleHandler.VisibleChatGO);
    }
}
