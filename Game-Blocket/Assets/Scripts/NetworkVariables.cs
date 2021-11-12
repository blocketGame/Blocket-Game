using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.NetworkVariable;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TerrainChunk;

/// <summary>
/// This class should be alsways synced
/// </summary>
public class NetworkVariables : NetworkBehaviour
{
	public static readonly bool debugNetworkVariables = true;

	public NetworkDictionary<Vector2Int, Chunk> chunks = new NetworkDictionary<Vector2Int, Chunk>(new NetworkVariableSettings
	{
		WritePermission = NetworkVariablePermission.Everyone,
		ReadPermission = NetworkVariablePermission.Everyone
	});

	private void Awake(){
		GlobalVariables.NetworkVariables = this;
		chunks.OnDictionaryChanged += OnNetorkWorldChanged;
	}

    private void OnNetorkWorldChanged(NetworkDictionaryEvent<Vector2Int, Chunk> changeEvent)
    {
		Debug.Log(changeEvent.ToString());
    }

    public void FixedUpdate()
	{
		if(debugNetworkVariables)
			Debug.Log(chunks.Values.Count);
	}

}
