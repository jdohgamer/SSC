using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;

public class GUIController: MonoBehaviour
{
	private CustomGameClient GameClientInstance;
	public string AppId;            // set in inspector
	// this is called when the client loaded and is ready to start

	EventData data;
	void Awake()
	{
		data = new EventData();
		//data.Code = CustomEventCode.GetFucked;
	}
	void Start()
	{
		GameClientInstance = new CustomGameClient();
		GameClientInstance.AppId = AppId;  // edit this!
		
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

	public void GetFuckedButton()
	{
		GameClientInstance.OpRaiseEvent(1, data, false, RaiseEventOptions.Default);
	}

	void MyCreateRoom(string roomName, byte maxPlayers)
	{
		GameClientInstance.OpCreateRoom(roomName, new RoomOptions() { MaxPlayers = maxPlayers }, TypedLobby.Default);
	}
}