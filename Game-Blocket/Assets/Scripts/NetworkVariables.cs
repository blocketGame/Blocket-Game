using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.NetworkVariable;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TerrainChunk;
using MLAPI.Messaging;

/// <summary>
/// This class should be alsways synced
/// </summary>
public class NetworkVariables : NetworkBehaviour
{

	private void Awake(){
		GlobalVariables.NetworkVariables = this;
	}

    public void FixedUpdate()
	{

		//if (Input.GetKeyDown(KeyCode.P))
			//PingServerServerRpc($"Hello from {NetworkManager.Singleton.ConnectedHostname}");
	}

}
