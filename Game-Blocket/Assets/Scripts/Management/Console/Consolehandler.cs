using System;
using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Experimental.GraphView;

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
            Command command = Command.GetCommand(str.Substring(1));
            if(command == null)
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


    private class Command{

        public static List<Command> Commands { get; } = new List<Command>()
        {
            new Command("ping"){ action = (str) => PrintToChat("Pong")},
            new Command("summon"){ action = (str) => { 
                // /summon 1 ~ ~ ~
                string[] arguments = CutCmd(str).Split(' ');
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
            new Command("kill"){ action = (x) => {
                GlobalVariables.LocalPlayer.transform.position = new Vector3(0, 10);
            } },
            new Command("gamemode"){ action = (x) => {
                sbyte str = (sbyte.TryParse(x.Split(' ')[1], out sbyte res) ? res : (sbyte)-1) ;
                if(str == -1){
                    PrintToChat($"\"{str}\" not a number! Use 0 for survival and 1 for creative!");
                    return;
                }
                switch(str){
                    case 0: PlayerVariables.Gamemode = Gamemode.SURVIVAL;
                    break;
                    case 1: PlayerVariables.Gamemode = Gamemode.CREATIVE;
                    break;
                    default:PrintToChat($"Gamemode {str} not found!");
                    break;
                }
                 
            }, Synms = { "gm", "gamem" } }
        };

        //Returns only the value after
        public static string CutCmd(string cmd){
            if (cmd.StartsWith(@"/"))
                cmd = cmd.Substring(1);
            return cmd.Trim();
        }

        public static Command GetCommand(string fullCommand){
            string cmdName = CutCmd(fullCommand.Split(' ')[0]);
            foreach(Command com in Commands)
                if(com.name == cmdName || com.Synms.Contains(cmdName))
                    return com;
            return null;
        }
    
        public List<string> Synms { get; set; } = new List<string>();

        public string name;

        public Action<string> action;
        
        public Command(List<string> prefix){
            name = prefix[0];
            Synms.AddRange(prefix);
        }
        public Command(string prefix) {
            name = prefix;
        }
        public Command(){}
    }

}
