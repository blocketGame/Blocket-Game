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

	public Profile(string name, int? profileHash)
    {
		this.name = name;
		this.profileHash = profileHash ?? new System.Random().Next();
	}
}

public class SettingsProfile : Profile{

	public Dictionary<string, KeyCode> Keys { get; set; } = new Dictionary<string, KeyCode>() {
		///Default Keys:
		{"InventoryKey",KeyCode.E},
		{"MainInteractionKey", KeyCode.Mouse0 },
		{"SideInteractionKey", KeyCode.Mouse1 },
		{"JumpKey", KeyCode.Space },
		{"RollKey", KeyCode.LeftControl },
		{"CrawlKey", KeyCode.LeftShift }
	};
	public SettingsProfile(string name, int? profileHash) : base(name, profileHash) { }
}

