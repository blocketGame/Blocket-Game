using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using static PlayerProfile;
using static WorldProfile;

/// <summary>
/// UNDONE
/// Will be used for Saving and Loading data<br></br>
/// <b>2 Sections: </b><br></br>
/// Import/Export=> From/ To File <br></br>
/// Sava/Load => From/ To Component
/// </summary>
public static class ProfileHandler {

	public static string MainParent => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.blocket";

	/// <summary>The Parent Directory for all profiles</summary>
	public static string ProfileParent => @$"{MainParent}\Profiles";
	public static string PlayerProfileParent => @$"{ProfileParent}\Players";

	#region ProfileHandling
	/// <summary></summary>
	public static void CheckParent() {
		if (!Directory.Exists(ProfileParent))
			Directory.CreateDirectory(ProfileParent);
		if (!Directory.Exists(PlayerProfileParent))
			Directory.CreateDirectory(PlayerProfileParent);
		if (!Directory.Exists(WorldProfileParent))
			Directory.CreateDirectory(WorldProfileParent);
		//foreach(string f in Directory.GetFiles(playerProfileParent))
		//	Debug.Log(f + File.Exists(f));
	}

	/// <summary>Exports the profile played now</summary>
	public static void ExportProfile(Profile p, bool player) {
		string prePath = player ? PlayerProfileParent : WorldProfileParent;

		CheckParent();
		string strToWrite = JsonUtility.ToJson(p, true);
		if (p.name == null) {
			Debug.LogWarning((player ? "Player" : "World") + "profile null!");
			p.name = player ? ListContentUI.selectedBtnNameCharacter : ListContentUI.selectedBtnNameWorld;
		}
		File.WriteAllText(prePath + @$"\{p.name}.json", strToWrite);
	}


	/// <summary>Imports the Profile played now</summary>
	public static Profile ImportProfile(string profileName, bool player) {
		CheckParent();
		string data = string.Empty;
		foreach (string iString in FindAllProfiles(player)) {
			int x = iString.LastIndexOf(@"\"), y = iString.LastIndexOf('.');
			if (iString.Substring(x + 1, y - x - 1).Equals(profileName)) {
				string path = iString;
				//Readoperation

				if (!File.Exists(path))
					throw new IOException("File not Found");
				data = File.ReadAllText(path);
				break;
			}
		}
		return data.Trim() == string.Empty
			? null
			: player ? JsonUtility.FromJson<PlayerProfile>(data) : (Profile)JsonUtility.FromJson<WorldProfile>(data);
	}

	/// <summary>Recognises all Profiles in the Parent dir</summary>
	/// <returns></returns>
	public static List<string> FindAllProfiles(bool player) {
		CheckParent();
		List<string> temp = new List<string>();
		string path = player ? PlayerProfileParent : WorldProfileParent;
		foreach (string iString in Directory.GetFiles(path))
			if (iString.EndsWith(".json"))
				temp.Add(iString);
		return temp;
	}

	#region Worldprofiles

	#endregion
	#endregion
}