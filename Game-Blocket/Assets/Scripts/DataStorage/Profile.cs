using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Used for Saving and Storing Data
/// </summary>
[Serializable]
public abstract class Profile{
	public int profileHash;
	public string name;

	public Profile(string name, int? profileHash = null)
    {
		this.name = name;
		this.profileHash = profileHash ?? new System.Random().Next();
	}
}

public class SettingsProfile : Profile{

	/// <summary>
	/// Default Keys for the whole game
	/// </summary>
	public IEnumerable<KeyTupple> LockKeysExceptSpecified(KeyCode k)
	{
		for (int i = 0; i < KeyValues.Count; i++)
		{
			KeyTupple kt = KeyValues[i];
			kt.MappedKey = (KeyValues[i].MappedKey == k ? k : KeyCode.None);
			yield return kt;
		}
    }

	public static List<KeyTupple> FillKeyValues()
	{
		return new List<KeyTupple> {
			new KeyTupple("InventoryKey", KeyCode.E),
			new KeyTupple("ChatKey", KeyCode.T),
			new KeyTupple("MainInteractionKey", KeyCode.Mouse0),
			new KeyTupple("SideInteractionKey", KeyCode.Mouse1),
			new KeyTupple("JumpKey", KeyCode.Space),
			new KeyTupple("RollKey", KeyCode.LeftControl),
			new KeyTupple("CrawlKey", KeyCode.LeftShift),
			new KeyTupple("CraftingInterface", KeyCode.F)
		};
	} 
	public List<KeyTupple> KeyValues { get; set; } = new List<KeyTupple>();

	public KeyCode InventoryKey => KeyValues[0].MappedKey;
	public KeyCode ChatKey => KeyValues[1].MappedKey;
	public KeyCode MainInteractionKey => KeyValues[2].MappedKey;
	public KeyCode SideInteractionKey => KeyValues[3].MappedKey;
	public KeyCode JumpKey => KeyValues[4].MappedKey;
	public KeyCode RollKey => KeyValues[5].MappedKey;
	public KeyCode CrawlKey => KeyValues[6].MappedKey;
	public KeyCode CraftingInterface => KeyValues[7].MappedKey;


	public SettingsProfile(string name, int? profileHash) : base(name, profileHash) { KeyValues = FillKeyValues(); }

}

public struct KeyTupple
{
	public string KeyName { get; set; }
	public KeyCode MappedKey { get; set; }
    public KeyTupple(string keyName , KeyCode mappedKey)
    {
		this.KeyName = keyName ?? string.Empty;
		this.MappedKey = mappedKey;
    }
}

