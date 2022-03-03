using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEditor.PlayerSettings;

public class MobHandler : MonoBehaviour
{
	public byte minSpawnDistance = 20;
	
	public ushort maxMobs;
	public int MobCache => mobsNow.Count;

	public List<Mob> mobsNow = new List<Mob>();

	private System.Random Random { get; set; }

	public Coroutine Coroutine { get; set; }

	public void Awake() {
		GlobalVariables.MobHandler = this;
		Random = new System.Random(GlobalVariables.WorldData?.Seed ?? 0);
	}

	public void FixedUpdate() {
		if (Coroutine == null && GameManager.State == GameState.INGAME)
			Coroutine = StartCoroutine(nameof(CheckEnviroment), null);
	}

	public IEnumerator CheckEnviroment(){
		while (true) {
			yield return new WaitForSeconds(1);
				if (MobCache >= maxMobs)
					continue;
				HandleRandomSpawning();
				Debug.Log("a");
				try
			{
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
		}
	}

	public void HandleRandomSpawning(){
		//UNFINISHED Mobspawnchance
		Mob mob = GlobalVariables.MobAssets.mobsInGame[Random.Next(0, GlobalVariables.MobAssets.mobsInGame.Count)];

		//Random Position
		Vector2Int posI = new Vector2Int(
		(Mathf.RoundToInt(GlobalVariables.LocalPlayerPos.x) + GetRandomAchsisFromLocalPlayer()) % WorldAssets.ChunkLength, 
		(Mathf.RoundToInt(GlobalVariables.LocalPlayerPos.y) + GetRandomAchsisFromLocalPlayer()) % WorldAssets.ChunkLength);

		if (Vector2.Distance(new Vector2(GlobalVariables.LocalPlayerPos.x, GlobalVariables.LocalPlayerPos.y), new Vector2(posI.x, posI.y)) < 20)
		return;


		Vector3? posSpawn = CheckSpaceAround(posI, new Vector2Int((int)mob.sizeX, (int)mob.sizeY));
		SpawnMob(mob.entityId, posSpawn);
	}

	private TerrainChunk GetTerrainChunkFromPos(Vector2Int posI) => GlobalVariables.TerrainHandler.GetChunkFromCoordinate(posI.x, posI.y);

	private Vector3? CheckSpaceAround(Vector2Int pos, Vector2Int mobSize){
		Debug.Log(pos);
		//If blockId is 0
		if (GetTerrainChunkFromPos(pos)?.blocks[Math.Abs(pos.x % WorldAssets.ChunkLength), Math.Abs(pos.y % WorldAssets.ChunkLength)] == 0)//If Block in terrain equals Air
		{
			return CheckSpaceAround(new Vector2Int(pos.x, pos.y - 1), mobSize); //Loop this method and try it below again
		}
		//TODO: offset
		byte spaceX = (byte)(GetLentgthOfSide(pos, Vector2Int.left) + GetLentgthOfSide(pos, Vector2Int.right) + 1);

		if (mobSize.x > spaceX)
			return null;

		for(int i = (mobSize.x - mobSize.x % 2) / -2; i < Mathf.FloorToInt(((float)mobSize.x - 1)/2); i++){ //1-2 times -- until it reaches half of the mobs size
			//UNFISHIED
			if (GetLentgthOfSide(new Vector2Int(pos.x + i, pos.y), Vector2Int.up) < mobSize.y)
				return null;
		}
		Debug.Log("Here");
		//return new Vector3(pos.x/ (mobSize.x%2 == 0 ? 1 : 2), pos.y / (mobSize.y % 2 == 0 ? 1 : 2));
		return new Vector3(pos.x , pos.y );
	}

	private byte GetLentgthOfSide(Vector2Int pos, Vector2Int pivot){
		byte sum = 0;
		for(int i = 0; i < byte.MaxValue-1; i++){
			//Set Querying Var
			Vector2Int? posI = null;
			if(pivot.x > 0)
				posI = new Vector2Int(pos.x +1, pos.y);
			if(pivot.x < 0)
				posI = new Vector2Int(pos.x -1, pos.y);
			if(pivot.y > 0)
				posI = new Vector2Int(pos.x, pos.y+1);

			//Check
			if(posI == null)
				throw new NullReferenceException();
			if (GetTerrainChunkFromPos(pos)?.blocks[
			(int)(Math.Abs((float)(posI?.x)) % WorldAssets.ChunkLength), 
			(int)Mathf.Abs((float)(posI?.y) % WorldAssets.ChunkLength)] == 0)
				sum++;
			else
				break;
		}
		return sum;
	}



	private int GetRandomAchsisFromLocalPlayer() => Random.Next(-GlobalVariables.WorldData.ChunkDistance* WorldAssets.ChunkLength, GlobalVariables.WorldData.ChunkDistance* WorldAssets.ChunkLength);


	private void SpawnMob(uint entityId, Vector3? position)=>SpawnMob(GlobalVariables.MobAssets.GetMobFromID(entityId, false), position);
	
	

	private void SpawnMob(Mob mob, Vector3? position)
	{
		if (position == null && mobsNow.Count>=maxMobs)
			return;
		Vector3 pos = position ?? Vector3.zero;
		if (pos == Vector3.zero) return;
		GameObject mgo = Instantiate(GlobalVariables.PrefabAssets.mobEntity, pos, Quaternion.identity);
		FlyingEnemyBehaviour fb =mgo.AddComponent<FlyingEnemyBehaviour>();
		fb.currentchunk = GetTerrainChunkFromPos(new Vector2Int((int)position?.x, (int)position?.y));
		mgo.transform.parent = GetTerrainChunkFromPos(new Vector2Int((int)position?.x, (int)position?.y)).ParentGO.transform;
		SpriteRenderer sr = mgo.AddComponent<SpriteRenderer>();
		sr.sprite = mob.sprite;

		BoxCollider2D bc = mgo.AddComponent<BoxCollider2D>();
		bc.size = mob.sprite.rect.size/100;
		if (DebugVariables.MobHandling)
			Debug.Log("MobSpawning");
	}
}
