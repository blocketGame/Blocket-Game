using System;
using UnityEngine;

[Serializable]
public class Mob : Entity{
	public Sprite sprite;
	[Tooltip("Higher => more rare")]
	public double spawnChance = 100;
	public int damage;
	public ushort maxHealth;
	public float HealthNow { get; set; }
	public MobKind mobKind;

	//TODO: More Detail
	public uint[] itemsEquiped;
	public Biom[] inBiomSpawning;
}

public enum MobKind{
	FRIENDLY, NEUTRAL, AGGRESIVE, BOSS
}
