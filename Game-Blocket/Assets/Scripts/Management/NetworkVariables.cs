
using Unity.Netcode;

using UnityEngine;
/// <summary>
/// This class should be alsways synced
/// </summary>
public class NetworkVariables : MonoBehaviour
{
	public static NetworkVariables Singleton { get; private set; }
	#region Multiplayer
	public static string ipAddress = UILobby.GetLocalIPAddress();
	public static ushort portAddress = 7777;
	#endregion

	private void Awake() => Singleton = this;

    public void FixedUpdate()
	{

		//if (Input.GetKeyDown(KeyCode.P))
			//PingServerServerRpc($"Hello from {NetworkManager.Singleton.ConnectedHostname}");
	}

}
