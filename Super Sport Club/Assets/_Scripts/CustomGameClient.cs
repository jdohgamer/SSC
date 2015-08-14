using System.Collections;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

//public class SaveGameInfo
//{
//	public int MyPlayerId;
//	public string RoomName;
//	public string DisplayName;
//	public bool MyTurn;
//	public Dictionary<string, object> AvailableProperties;
//	
//	public string ToStringFull()
//	{
//		return string.Format("\"{0}\"[{1}] {2} ({3})", RoomName, MyPlayerId, MyTurn, SupportClass.DictionaryToString(AvailableProperties));
//	}
//}
public class CustomEventCode: EventCode
{
	// <summary>(230) Initial list of RoomInfos (in lobby on Master)</summary>
	public const byte GetFucked = 1;
}
public class CustomGameClient : LoadBalancingClient 
{
	public const string PropTurn = "turn";
	public const string PropNames = "names";
	public Grid_Setup board;

	public void GetFucked()
	{
		OpRaiseEvent((byte)1, null, true, new RaiseEventOptions(){ CachingOption = EventCaching.AddToRoomCache });
	}

	public override void OnOperationResponse(OperationResponse operationResponse)
	{
		base.OnOperationResponse(operationResponse);
		
		switch (operationResponse.OperationCode)
		{
		case (byte)OperationCode.WebRpc:
			if (operationResponse.ReturnCode == 0)
			{
				//this.OnWebRpcResponse(new WebRpcResponse(operationResponse));
			}
			break;
		case (byte)OperationCode.JoinGame:
		case (byte)OperationCode.CreateGame:
			if (operationResponse.ReturnCode != 0)
			{
				Debug.Log(string.Format("Join or Create failed for: '{2}' Code: {0} Msg: {1}", operationResponse.ReturnCode, operationResponse.DebugMessage, this.CurrentRoom));
			}
			if (this.Server == ServerConnection.GameServer)
			{
				if (operationResponse.ReturnCode == 0)
				{
					this.LoadBoardFromProperties(false);
				}
			}
			break;
		case (byte)OperationCode.JoinRandomGame:
			if (operationResponse.ReturnCode == ErrorCode.NoRandomMatchFound)
			{
				// no room found: we create one!
				this.CreateTurnbasedRoom();
				Debug.Log("New room created");
			}
			break;
		}
	}
	
	
	public override void OnEvent(EventData photonEvent)
	{
		base.OnEvent(photonEvent);
		
		switch (photonEvent.Code)
		{
		case EventCode.PropertiesChanged:
			//Debug.Log("Got Properties via Event. Update board by room props.");
			//this.LoadBoardFromProperties(true);
			//this.board.ShowFlippedTiles();
			break;
		case CustomEventCode.GetFucked:
			Debug.Log("Go and get yourself a good fucking");
			//Debug.Log("Got Properties via Event. Update board by room props.");
			//this.LoadBoardFromProperties(true);
			//this.board.ShowFlippedTiles();
			break;
		case EventCode.Join:
			if (this.CurrentRoom.Players.Count == 2 && this.CurrentRoom.IsOpen)
			{
				this.CurrentRoom.IsOpen = false;
				this.CurrentRoom.IsVisible = false;
				//this.SavePlayersInProps();
			}
			break;
		case EventCode.Leave:
			//if (this.CurrentRoom.Players.Count == 1 && !this.GameWasAbandoned)
			{
				this.CurrentRoom.IsOpen = true;
				this.CurrentRoom.IsVisible = true;
			}
			break;
		}
	}

	public void LoadBoardFromProperties(bool calledByEvent)
	{
		//board.InitializeBoard();
		
		Hashtable roomProps = this.CurrentRoom.CustomProperties;
		Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));

		if (roomProps.Count == 0)
		{
			// we are in a fresh room with no saved board.
			board.Generate();
			this.SaveBoardToProperties();
			Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));
		}

		
		// we are in a game that has props (a board). read those (as update or as init, depending on calledByEvent)
		bool success = board.SetBoardByCustomProperties(roomProps, calledByEvent);
		if (!success)
		{
			Debug.LogError("Not loaded board from props?");
		}
		
		/*
		// we set properties "pt" (player turn) and "t#" (turn number). those props might have changed
		// it's easier to use a variable in gui, so read the latter property now
		if (this.CurrentRoom.CustomProperties.ContainsKey("t#"))
		{
			this.TurnNumber = (int) this.CurrentRoom.CustomProperties["t#"];
		}
		else
		{
			this.TurnNumber = 1;
		}
		
		if (this.CurrentRoom.CustomProperties.ContainsKey("pt"))
		{
			this.PlayerIdToMakeThisTurn = (int) this.CurrentRoom.CustomProperties["pt"];
			//Debug.Log("This turn was played by player.ID: " + this.PlayerIdToMakeThisTurn);
		}
		else
		{
			this.PlayerIdToMakeThisTurn = 0;
		}
		
		// if the game didn't save a player's turn yet (it is 0): use master
		if (this.PlayerIdToMakeThisTurn == 0)
		{
			this.PlayerIdToMakeThisTurn = this.CurrentRoom.MasterClientId;
		}
		
		this.MyPoints = GetPlayerPointsFromProps(this.LocalPlayer);
		this.OthersPoints = GetPlayerPointsFromProps(this.Opponent);
		*/
	}

	public void CreateTurnbasedRoom()
	{
		string newRoomName = string.Format("{0}-{1}", this.NickName, Random.Range(0,1000).ToString("D4"));    // for int, Random.Range is max-exclusive!
		Debug.Log(string.Format("CreateTurnbasedRoom(): {0}", newRoomName));
		
		RoomOptions roomOptions = new RoomOptions()
		{
			MaxPlayers = 2,
			CustomRoomPropertiesForLobby = new string[] { PropTurn, PropNames },
			PlayerTtl = int.MaxValue,
			EmptyRoomTtl = 5000
		};
		this.OpCreateRoom(newRoomName, roomOptions, TypedLobby.Default);
	}

	public void SaveBoardToProperties()
	{
		Hashtable boardProps = board.GetBoardAsCustomProperties();
		//boardProps.Add("pt", this.PlayerIdToMakeThisTurn);  // "pt" is for "player turn" and contains the ID/actorNumber of the player who's turn it is
		//boardProps.Add("t#", this.TurnNumber);
		boardProps.Add("tx#", board.width);
		boardProps.Add("tz#", board.length);
		//boardProps.Add(GetPlayerPointsPropKey(this.LocalPlayer.ID), this.MyPoints); // we always only save "our" points. this will not affect the opponent's score.
		
		
		// our turn will be over if 2 tiles are clicked/flipped but not the same. in that case, we update the other player if inactive
		bool webForwardToPush = false;

		//Debug.Log(string.Format("saved board to room-props {0}", SupportClass.DictionaryToString(boardProps)));
		this.OpSetCustomPropertiesOfRoom(boardProps, webForwardToPush);
	}
}
