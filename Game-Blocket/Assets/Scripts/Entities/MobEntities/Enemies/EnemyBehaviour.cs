using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // Start is called before the first frame update
    void Awake()
    {
        //MaxHealth = MobAssets.Singleton.GetMobFromID(MobID,false).maxHealth;
        //Health = MaxHealth;
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
            gameObject.GetComponent<Rigidbody2D>().transform.position = gameObject.GetComponent<Rigidbody2D>().transform.position + Vector3.up;
        }
    }
}
