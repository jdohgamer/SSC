using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;

public class GUIController: MonoBehaviour
{
	private CustomGameClient GameClientInstance;
	public string AppId;            // set in inspector
	// this is called when the client loaded and is ready to start

	public float serviceInterval = 1;
	public float timeSinceService;

	void Awake()
	{
	
	}
	void Start()
	{
		GameClientInstance = GameController.Instance.GameClientInstance;
		GameClientInstance.AppId = AppId;  // edit this!
	
	}

	
	void Update()
	{
		timeSinceService += Time.deltaTime;
		if (timeSinceService > serviceInterval)
		{
			this.GameClientInstance.Service();
			timeSinceService = 0;
		}
	}
	
	void OnApplicationQuit()
	{
		GameClientInstance.Disconnect();
	}
	public void NewGameButton()
	{
		this.GameClientInstance.CreateTurnbasedRoom();
		//this.GameClientInstance.OpJoinRandomRoom(null, 0);
	}

	public void GetFuckedButton()
	{
		Debug.Log("Fuck you");
		this.GameClientInstance.GetFucked();

	}

	void MyCreateRoom(string roomName, byte maxPlayers)
	{
		GameClientInstance.OpCreateRoom(roomName, new RoomOptions() { MaxPlayers = maxPlayers }, TypedLobby.Default);
	}
}