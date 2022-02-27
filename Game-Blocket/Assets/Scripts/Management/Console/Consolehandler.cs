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

    /// <summary>Used for imputfields</summary>
    /// <param name="str">Input</param>
    public static void Handle(string str){
        str = PrepareString(str, out bool isCommand);
        if (string.IsNullOrEmpty(str))
            return;
        if (isCommand){
            Command? command = Command.GetCommand(str.Substring(1));
            if(command != null)
                PrintToChat($"Command: \"{str}\" not found");
            command?.action(str);
        }
        else
            PrintToChat(str);
    }

    /// <summary>Self explaining</summary>
    /// <param name="str">String to print</param>
    /// <exception cref="NullReferenceException">If Prefab has no Text Component</exception>
    private static void PrintToChat(string str){
        if(DebugVariables.ShowChatEntries)
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


    public struct Command{

        public static List<Command> Commands { get; } = new List<Command>()
        {
            new Command(){ prefix = "ping", action = (str) => PrintToChat("Pong")},
            new Command(){ prefix = "summon", action = (str) => { 
                // /summon 1 ~ ~ ~
                string[] arguments = Commands[1].CutCmd(str).Split(' ');
                try{
                    //Enemy id
                    int enemyId = int.Parse(arguments[0]);
                    //X
                    int x = int.Parse(arguments[1]);
                    //Y
                    int y = int.Parse(arguments[2]);
                    //Z
                    int z = int.Parse(arguments[3]);
                    
                    //Forget enemyhandler lol
                }catch(Exception e){
                    PrintToChat(str: e.ToString());
                }
            } },
            new Command(){ prefix = "kill", action = (x) => {
                GlobalVariables.LocalPlayer.transform.position = new Vector3(0, 10);
            } }
        };

        //Returns only the value after
        public string CutCmd(string cmd){
            if (cmd.StartsWith(@"/"))
                cmd = cmd.Substring(1);
            cmd = cmd.Substring(prefix.Length);
            return cmd.Trim();
        }

        public static Command? GetCommand(string fullCommand){
            foreach(Command command in Commands)
                if(fullCommand.StartsWith(command.prefix))  
                    return command;
            return null;
        }
    
        public string prefix;

        public Action<string> action;

    }

}
