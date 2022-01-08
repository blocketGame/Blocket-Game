using System;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handlers the Chat
/// TODO: Change to Chathandler, Multiplayer
/// </summary>
public static class ConsoleHandler{

    public static GameObject VisibleChatGO { get; private set; }

    /// <summary>Stores all commands</summary>
    public static Dictionary<string, Action> AllCommands { get; } = new Dictionary<string, Action>(){
        {"ping" , () => PrintToChat("Pong")}
    };

    /// <summary>Used for imputfields</summary>
    /// <param name="str">Input</param>
    public static void Handle(string str){
        str = PrepareString(str, out bool isCommand);
        if (string.IsNullOrEmpty(str))
            return;
        if (isCommand)
            if (AllCommands.TryGetValue(str.Substring(1), out Action ac))
                ac();
            else
                PrintToChat($"Command: \"{str}\" not found");
        else
            PrintToChat(str);
    }

    /// <summary>Self explaining</summary>
    /// <param name="str">String to print</param>
    /// <exception cref="NullReferenceException">If Prefab has no Text Component</exception>
    private static void PrintToChat(string str){
        if(DebugVariables.ShowChatEntrys)
            Debug.Log($"Chat: {str}");

        if (VisibleChatGO != null)
            UnityEngine.Object.Destroy(VisibleChatGO);
        GameObject textGO = UnityEngine.Object.Instantiate(GlobalVariables.PrefabAssets.consoleText, GlobalVariables.UIInventory.chatParent.parent.position, Quaternion.identity ,GlobalVariables.UIInventory.chatParent.parent);
        Text text = textGO.TryGetComponent(out Text t) ? t : throw new NullReferenceException();
        text.text = GetPlayerPrefix() + str;
        VisibleChatGO = textGO;
    }



    /// <summary>
    /// TODO: Use real Playernames<br></br><seealso cref="PlayerProfile"/>
    /// </summary>
    /// <returns></returns>
    private static string GetPlayerPrefix(){
        return "Dev > ";
    }
    
    /// <summary>Prepares a string and checks if it is a command</summary>
    /// <param name="str">Input</param>
    /// <param name="isCommand">if the chat is a command (with "/" before)</param>
    /// <returns>prepared string</returns>
    private static string PrepareString(string str, out bool isCommand){
        isCommand = str.StartsWith(@"/");
        if (isCommand)
            str = str.ToLower();
        return str.Trim();
    }

}
