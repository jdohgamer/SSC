using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;

public class GUIController: MonoBehaviour
{
	private CustomGameClient GameClientInstance;
	public string AppId;            // set in inspector
	// this is called when the client loaded and is ready to start
	public Grid_Setup board;
	EventData data;
	void Awake()
	{
		GameClientInstance = new CustomGameClient();
		GameClientInstance.AppId = AppId;  // edit this!
		GameClientInstance.board = board;
	}
	void Start()
	{

		Application.runInBackground = true;
		CustomTypes.Register();
		// "eu" is the European region's token
		bool connectInProcess = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors
	}
	
	void Update()
	{
		GameClientInstance.Service();
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