using System.Collections;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

public class CustomGameClient : LoadBalancingClient 
{
	public static CustomGameClient ClientInstance;
	public const byte EndTurn = 1;
	public const byte Execute = 2;
	public const byte SubmitTeam = 3;
	public const string PropTurn = "turn";
	public const string PropNames = "names";
	public bool GameWasAbandoned
	{
		get { return this.CurrentRoom != null && this.CurrentRoom.Players.Count < 2 && this.CurrentRoom.CustomProperties.ContainsKey("t#"); }
	}
	public Player Opponent
	{
		get
		{
			Player opp = this.LocalPlayer.GetNext();
			//Debug.Log("you: " + this.LocalPlayer.ToString() + " other: " + opp.ToString());
			return opp;
		}
	}

	public MainGame mainGame;
	public byte MaxActions = 5;
	public bool bTurnDone;
	public int team;
	bool P1Submitted, P2Submitted;
	int turnNumber;
	Hashtable oppHT;

	public CustomGameClient()
	{

	}
	
	public void EndTurnEvent(PlayerAction[] actions)
	{
		Hashtable content = GetActionsAsProps (actions);
		if (IsPlayerOne()) 
		{
			P1Submitted = true;
			this.loadBalancingPeer.OpRaiseEvent(EndTurn, null, true, null);
			if(BothPlayersHaveSubmitted())
			{
				mainGame.CalcMoves(); //
				P1Submitted = false;
				P2Submitted = false;
				Debug.Log("Go and get yourself a good fucking");
			}
			//ExecuteMoves(content);
		}else{
			P2Submitted=true;
			this.loadBalancingPeer.OpRaiseEvent(EndTurn, content, true, null);
		}
		
	}
	public void SubmitTeamEvent(Team t)
	{	
		if (IsPlayerOne()) 
		{
			P1Submitted = true;
		}else{
			P2Submitted=true;
		}
		Hashtable TeamHT = t.GetTeamAsProps();
	
		this.loadBalancingPeer.OpRaiseEvent(SubmitTeam, TeamHT, true, null);
		if(BothPlayersHaveSubmitted())
		{
			mainGame.LoadCharactersFromProps(oppHT);
			P1Submitted = false;
			P2Submitted = false;
		}
			
	}

	bool IsPlayerOne()
	{
		return this.LocalPlayer.IsMasterClient;
	}
	public bool HasOppSubmitted()
	{
		if (IsPlayerOne ()) 
		{
			return P2Submitted;
		} else
			return P1Submitted;
	}
	bool BothPlayersHaveSubmitted()
	{
		return P1Submitted && P2Submitted;
	}
		
	Hashtable GetActionsAsProps(PlayerAction[] actions)
	{
		Hashtable MoveSet = new Hashtable ();
		for (int i = 0; i<actions.Length; i++) 
		{
			if(actions[i]!=null)
			{
				MoveSet.Add(i.ToString(),actions[i].GetActionProp());
			}
		}
		return MoveSet;
	}

	PlayerAction[] LoadActionsFromProps(Hashtable ht)
	{
		PlayerAction[] actions = new PlayerAction[ht.Count];
		for(int i = 0;i<ht.Count;i++)
		{
			if(ht[i.ToString()]!=null)
			{
				Hashtable ion = ht[i.ToString()]as Hashtable;
				actions[i] = PlayerAction.GetActionFromProps(ion);
			}
		}
		return actions;
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
			case (byte)OperationCode.Authenticate:
			//mainGame.StartCoroutine("NewOnlineGame");
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
					if(IsPlayerOne())
					{
						mainGame.CurrentTeamNum = 0;
						//mainGame.NewGame();
					}else mainGame.CurrentTeamNum  = 1;
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
		Debug.Log(operationResponse.OperationCode.ToString());
	}
	
	
	public override void OnEvent(EventData photonEvent)
	{
		base.OnEvent(photonEvent);
		
		switch ((byte)photonEvent.Code)
		{
			case (byte)EndTurn:
			{
				if(IsPlayerOne())
				{
					P2Submitted = true;
					object content = photonEvent.Parameters[ParameterCode.CustomEventContent];
					Hashtable turnClick = content as Hashtable;
					mainGame.SetOtherTeamActions(turnClick);
					if(BothPlayersHaveSubmitted())
					{
						mainGame.CalcMoves();
						P1Submitted = false;
					P2Submitted = false;
						Debug.Log("Go and get yourself a good fucking");
					}
				}else{P1Submitted=true;}	
				break;
			}
			case (byte)SubmitTeam:
			{
				if(IsPlayerOne())
				{
					P2Submitted = true;
				}else{P1Submitted=true;}
				
				object content = photonEvent.Parameters[ParameterCode.CustomEventContent];
				oppHT = content as Hashtable;

				if(BothPlayersHaveSubmitted())
				{
					mainGame.LoadCharactersFromProps(oppHT);
					P1Submitted = false;
					P2Submitted = false;
				}
				//oppCharacers = LoadCharactersFromProps(turnClick);
				//SetTeams(oppHT);
				
				break;
			}
			case EventCode.PropertiesChanged:
				//Debug.Log("Got Properties via Event. Update board by room props.");
				this.LoadBoardFromProperties(true);
				break;
			case (byte)Execute:
			{
				object content = photonEvent.Parameters[ParameterCode.CustomEventContent];
				Hashtable turnClick = content as Hashtable;
				mainGame.ExecuteMoves(turnClick);
				break;
			}
			case EventCode.Join:
				if (this.CurrentRoom.Players.Count == 2 && this.CurrentRoom.IsOpen)
				{
					this.CurrentRoom.IsOpen = false;
					this.CurrentRoom.IsVisible = false;
					Debug.Log("Some faggot joined the room");
					this.SavePlayersInProps();
				}
				break;
			case EventCode.Leave:
				if (this.CurrentRoom.Players.Count == 1 && !this.GameWasAbandoned)
				{
					this.CurrentRoom.IsOpen = true;
					this.CurrentRoom.IsVisible = true;
				}
				break;
		}
		Debug.Log(photonEvent.Code.ToString());
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
	
	void SetTeams(Hashtable HT)
	{
		mainGame.LoadCharactersFromProps(HT);
		if(BothPlayersHaveSubmitted())
		{
			
		}
	}
	
	public void SaveBoardToProperties()
	{
		Hashtable boardProps = Grid_Setup.Instance.GetBoardAsCustomProperties();

		
		//boardProps.Add(GetPlayerPointsPropKey(this.LocalPlayer.ID), this.MyPoints); // we always only save "our" points. this will not affect the opponent's score.
		bool webForwardToPush = false;

		//Debug.Log(string.Format("saved board to room-props {0}", SupportClass.DictionaryToString(boardProps)));
		this.OpSetCustomPropertiesOfRoom(boardProps, webForwardToPush);
	}
	public void LoadBoardFromProperties(bool calledByEvent)
	{	
		Hashtable roomProps = this.CurrentRoom.CustomProperties;
		Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));

		if (roomProps.Count == 0)
		{
			// we are in a fresh room with no saved board.
			mainGame.NewGame();
			this.SaveBoardToProperties();
			Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));
		}

		// we are in a game that has props (a board). read those (as update or as init, depending on calledByEvent)
		bool success = Grid_Setup.Instance.SetBoardByCustomProperties(roomProps, calledByEvent);
		if (!success)
		{
			Debug.LogError("Not loaded board from props?");
		}
		//this.myTeam = MainGame.Instance.Teams [team];

		// we set properties "pt" (player turn) and "t#" (turn number). those props might have changed
		// it's easier to use a variable in gui, so read the latter property now
		if (this.CurrentRoom.CustomProperties.ContainsKey("t#"))
		{
			this.turnNumber = (int) this.CurrentRoom.CustomProperties["t#"];
		}
		else
		{
			this.turnNumber = 1;
		}
		/*
		this.MyPoints = GetPlayerPointsFromProps(this.LocalPlayer);
		this.OthersPoints = GetPlayerPointsFromProps(this.Opponent);
		*/
	}

	public void SavePlayersInProps()
	{
		if (this.CurrentRoom == null || this.CurrentRoom.CustomProperties == null || this.CurrentRoom.CustomProperties.ContainsKey(PropNames))
		{
			Debug.Log("Skipped saving names. They are already saved.");
			return;
		}
		
		Debug.Log("Saving names.");
		Hashtable boardProps = new Hashtable();
		boardProps[PropNames] = string.Format("{0};{1}", this.LocalPlayer.NickName, this.Opponent.NickName);
		this.OpSetCustomPropertiesOfRoom(boardProps, false);
	}

}
