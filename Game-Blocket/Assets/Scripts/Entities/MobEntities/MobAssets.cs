using System;
using System.Collections.Generic;
using UnityEngine;

public class MobAssets: MonoBehaviour{
	public List<Mob> mobsInGame;
	public List<Mob> bossesInGame;

	public Mob GetMobFromID(uint id, bool queryBosses){
		foreach (Mob m in mobsInGame)
			if (m.entityId == id)
				return m;
        return queryBosses ? GetBossFromID(id) : null;
    }

    public Mob GetBossFromID(uint id)
	{
		foreach (Mob m in mobsInGame)
			if (m.entityId == id)
				return m;
		return null;
	}

    public void Awake()
    {
		GlobalVariables.MobAssets = this;
    }

}


