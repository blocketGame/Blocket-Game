using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldAssets : MonoBehaviour
{
    /// <summary>Needs to be static! Else: BigRip</summary>
    public static byte ChunkLength => 32;

	public List<Biom> bioms = new List<Biom>();
	public List<BlockData> blocks = new List<BlockData> ();

	public void Awake()
    {
        GlobalVariables.WorldAssets = this;
		GlobalVariables.Structures.ReadAllStructures();
    }

    /// <summary>
    /// returns the BlockData object of the index
    /// </summary>
    /// <param name="id">index of the block</param>
    /// <returns></returns>
    public BlockData GetBlockbyId(byte id) {
		foreach (BlockData bd in blocks) {
			if (bd.blockID == id) {
				return bd;
			}
		}
		return blocks[0];
	}

	public byte GetBlockFromTile(TileBase tile) {
		foreach (BlockData b in blocks) {
			if (b.tile != null)
				if (b.tile.Equals(tile))
					return b.blockID;
		}
		return 0;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public List<Biom> GetBiomsByType(Biomtype type) {
		List<Biom> biomlist = new List<Biom>();
		foreach (Biom b in bioms) {
			if (b.Biomtype.Contains(type))
				biomlist.Add(b);
		}
		return biomlist;
	}

	#region Filehandling

	/// <summary>
	/// Creates Blocks.txt file as documentation for the blocks array
	/// </summary>

	public void PutBlocksIntoTxt() {
		string writeContent = "# This File is considered as documentation tool for the Blocks and their Ids \n";
		for (int x = 0; x < blocks.Count; x++) {
			writeContent += "\n" +
				" ID : " + blocks[x].blockID + "\n" +
				" Name : " + blocks[x].name + "\n";
		}

		File.WriteAllText("Docs/Blocks.txt", writeContent);
	}

    public void PutBiomsIntoTxt() {
        string writeContent = "# This File is considered as documentation tool for the Bioms and their Indizes \n";
        for (int x = 0; x < bioms.Count; x++) {
            writeContent += "\n" +
                " ID : " + bioms[x].Index + "\n" +
                " BiomName : " + bioms[x].BiomName + "\n";
            for (int y = 0; y < bioms[x].Regions.Length; y++) {
                writeContent += "\n" +
                "\t ID : " + bioms[x].Regions[y].BlockID + "\n" +
                "\t Range : " + bioms[x].Regions[y].RegionRange + "\n";
            }

        }

        writeContent += "\n ---------------------------- Rules ---------------------------------- \n Range : -1 => Infinity";

        File.WriteAllText("Docs/Bioms.txt", writeContent);
    }

    #endregion
}

[System.Serializable]
public struct BlockData {
	[Header("Name of the Block")]
	public string name;
	[Header("ID of the Block")]
	public byte blockID;
	[Header("Tile texture")]
	public TileBase tile;
	[Header("How long it takes to break the block (in seconds)")]
	public byte removeDuration;
	[Header("What the block could drop on destory")]
	public List<BlockDropAble> blockDrops;

	/// <summary>
	/// Specifies what the block drops
	/// </summary>
	[System.Serializable]
	public class BlockDropAble {
		[Header("ItemID of the Item what should be droped")]
		public uint itemID;
		[Header("Dropchance in percent")]
		[Range(1, 100)]
		public float dropchance;
		[Header("The item should drop if that Tooltype is used")]
		public ToolItem.ToolType toolItemType;
	}
}