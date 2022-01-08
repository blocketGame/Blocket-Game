using MLAPI;

/// <summary>
/// This class should be alsways synced
/// </summary>
public class NetworkVariables : NetworkBehaviour
{

	#region Multiplayer
	public static string ipAddress = UILobby.GetLocalIPAddress();
	public static int portAddress = 7777;
	public static bool muliplayer = false;
	#endregion

	private void Awake(){
	}

    public void FixedUpdate()
	{

		//if (Input.GetKeyDown(KeyCode.P))
			//PingServerServerRpc($"Hello from {NetworkManager.Singleton.ConnectedHostname}");
	}

}
