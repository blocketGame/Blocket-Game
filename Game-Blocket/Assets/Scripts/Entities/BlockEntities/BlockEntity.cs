using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockEntity : IStringLoadable {

    public byte blockId;

    public IStringLoadable Restore() {
        throw new System.NotImplementedException();
    }

    public string Store() {
        throw new System.NotImplementedException();
    }
}

public class BlockEntityData{
    public Vector2Int Vector2Int { get; set; }

    public List<ushort> Items { get; } = new List<ushort>();

    public Type Blocktype { get; set; }
}
