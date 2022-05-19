using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BlockData;

/// <summary>
/// Base Class for every Enemy
/// </summary>
[Serializable]
public abstract class EnemyBehaviour : MonoBehaviour
{
    public abstract int Damage { get; set; }
    public abstract int Health { get; set; }
    public abstract int MaxHealth { get; set; }
    public abstract int Regeneration { get; set; }

    public int regenerationticks=5; //every 5 secs Regen
    public virtual string deathAnimation { get; set; }
    public virtual Animator Animator { get; set; }
    public abstract uint MobID { get; set; }
    public abstract List<BlockDropAble> Drops { get; set; }


    // Start is called before the first frame update
    void Awake()
    {
        MaxHealth = MobAssets.Singleton.GetMobFromID(MobID,false).maxHealth;
        Health = MaxHealth;
        Debug.Log("Enemy");
        if (Drops == null)
            Drops = new List<BlockDropAble>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
            Death();
    }

    public void Death()
    {
        GameObject.Destroy(this.gameObject);
        //Play death anim
        //Leave drops
        TerrainChunk c = ClientTerrainHandler.Singleton.GetChunkFromCoordinate(transform.position.x, transform.position.y);
        foreach (BlockDropAble d in Drops) 
            if (UnityEngine.Random.Range(1f,100f) < d.dropchance && PlayerVariables.Dimension == Dimension.OVERWORLD)
                c.InstantiateDrop(new Vector3Int((int)transform.position.x%WorldAssets.ChunkLength,(int)transform.position.y%WorldAssets.ChunkHeight, (int)transform.position.z), (byte)d.count, d.itemID);
            else if(UnityEngine.Random.Range(1f, 100f) < d.dropchance)
            {
                Vector3Int coordinate = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);

                GameObject dropGO = new GameObject($"Drop ItemID: {d.itemID}");
                //dropGO.transform.SetParent(dropParent.transform);
                dropGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                dropGO.transform.position = new Vector3(coordinate.x + 0.5f, coordinate.y + 0.5f, 0);
                dropGO.layer = LayerMask.NameToLayer("Drops");

                Drop drop = dropGO.AddComponent<Drop>();
                drop.ItemId = d.itemID;
                drop.Count = (ushort)d.count;
                Debug.Log("Instantiate Drop");
            }
    }

    /// <summary>
	/// Is just used when chunks are unable to load (Player doesn't stay in Wall)
	/// </summary>
	private void PreventFloorGlitch()
    {
        if (PlayerVariables.Dimension == Dimension.DUNGEON)
            return;
        Vector3Int playerCell = WorldData.Singleton.Grid.WorldToCell(gameObject.GetComponent<Rigidbody2D>().transform.position);
        if (TerrainHandler.Singleton.GetBlockFormCoordinate(playerCell.x, playerCell.y) != 0)
        {
            Debug.LogWarning("UWU - WALL_GLITCH_HANDLED");
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            gameObject.GetComponent<Rigidbody2D>().transform.position = gameObject.GetComponent<Rigidbody2D>().transform.position + Vector3.up;
        }
        else if(gameObject.GetComponent<Rigidbody2D>().gravityScale == 0)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            gameObject.GetComponent<Rigidbody2D>().gravityScale = 1;
        }
    }
}
