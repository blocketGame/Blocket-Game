using MLAPI;

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
