using System.Collections.Generic;

using UnityEngine;

public class MobAssets: MonoBehaviour{
	public static MobAssets Singleton { get; private set; }

	public List<Mob> mobsInGame;
	public List<Mob> bossesInGame;

	public Mob GetMobFromID(uint id, bool queryBosses){
		foreach (Mob m in mobsInGame)
			if (m.entityId == id)
				return m;
        return queryBosses ? GetBossFromID(id) : throw new System.Exception($"No mob with ID: {id} found");
    }

    public Mob GetBossFromID(uint id)
	{
		foreach (Mob m in mobsInGame)
			if (m.entityId == id)
				return m;
		return null;
	}

	public void Awake() => Singleton = this;

}


