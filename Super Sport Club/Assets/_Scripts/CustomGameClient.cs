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
public class CustomGameClient : LoadBalancingClient 
{
	public const byte EndTurn = 1;
	public const byte Execute = 2;
	public const byte SubmitTeam = 3;
	public const string PropTurn = "turn";
	public const string PropNames = "names";
	public int TurnNumber{get{ return turnNumber;}}
	public Grid_Setup board;
	public byte MaxActions = 5;
	public bool bTurnDone;
	public GUIController gui;
	public FSM_Character[] myCharacters, oppCharacers;
	public Team.TeamNumber team;
	Team myTeam;
	PlayerAction[] myActions, oppActions;
	public bool P1Submitted, P2Submitted;
	int turnNumber;
	Message message;
	byte actionCount = 0;
	Hashtable oppHT;
	int characterCount = 0, maxCharacters = 10, teamSize = 5, teamOneSize, teamTwoSize;

	
	public CustomGameClient()
	{
		myActions = new PlayerAction[MaxActions];
		myCharacters = new FSM_Character[maxCharacters];
		oppCharacers = new FSM_Character[maxCharacters];
	}
	
	public void EndTurnEvent()
	{
		Hashtable content = GetActionsAsProps (myActions);
		if (IsPlayerOne()) 
		{
			P1Submitted = true;
			if(BothPlayersHaveSubmitted())
			{
				turnNumber++;
				CalcMoves();
				Debug.Log("Go and get yourself a good fucking");
			}
			//ExecuteMoves(content);
		}else{
			P2Submitted=true;
			this.loadBalancingPeer.OpRaiseEvent(EndTurn, content, true, null);
		}
		
	}
	public void SubmitTeamEvent()
	{	
		if (IsPlayerOne()) 
		{
			P1Submitted = true;
		}else{
			P2Submitted=true;
		}
		SetTeams(oppHT);
		Hashtable TeamHT = myTeam.GetTeamAsProps();
	
		this.loadBalancingPeer.OpRaiseEvent(SubmitTeam, TeamHT, true, null);
			
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


	public void SetPlayerAction(PlayerAction act)
	{
		if(actionCount<MaxActions&&act.iCh.actionCount<act.iCh.maxActions)
		{
			myActions[actionCount] = act;
			actionCount += 1;
			if(act.action == PlayerAction.Actions.Move)
			{
				act.iCh.SetMoveTarget(act.cTo);
			}
			if(act.action == PlayerAction.Actions.Pass)
			{
				act.iCh.SetPassTarget(act.cTo);
			}
		}
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
	FSM_Character[] LoadCharactersFromProps(Hashtable ht)
	{
		FSM_Character[] otherTeam = new FSM_Character[ht.Count];
		for(int i = 0;i<ht.Count;i++)
		{
			Hashtable hash = ht[i.ToString()]as Hashtable;
			int team = (int)hash["Team"];
			Vector3 loc = (Vector3)hash["Location"];
			string role = (string)hash["Name"];
			otherTeam[i] = Grid_Setup.Instance.SetCharacter(team, i, loc);
			//team[i].ReturnCharacter(hash);
		}return otherTeam;
	}
	PlayerAction[] LoadActionsFromProps(Hashtable ht)
	{
		PlayerAction[] actions = new PlayerAction[ht.Count];
		for(int i = 0;i<ht.Count;i++)
		{
			if(ht[i.ToString()]!=null)
			{
				Hashtable ion = ht[i.ToString()]as Hashtable;
//				PlayerAction.Actions act = (PlayerAction.Actions)ion["Act"];
//				int iChId = (int)ion["iCharacter"];
//				int iChTeam = (int)ion["iCharacterTeam"];
//				Cell cell =	Grid_Setup.Instance.GetCellByID((int)ion["tCell"]);
				actions[i] = PlayerAction.GetActionFromProps(ion);
			}
		}
		return actions;
	}

	public void ClearActions()
	{
		for(int c= 0;c<myActions.Length;c++)
		{
			if(myActions[c]!=null)
			{
				myActions[c] = null;
			}
		}
		board.TurnOffHiglightedAdjacent ();
		actionCount = 0;
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
					if(IsPlayerOne())
					{
						team = Team.TeamNumber.TeamOne;
					}else team = Team.TeamNumber.TeamTwo;
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
		
		switch ((byte)photonEvent.Code)
		{
			case (byte)EndTurn:
			{
				object content = photonEvent.Parameters[ParameterCode.CustomEventContent];
				Hashtable turnClick = content as Hashtable;
				oppActions = LoadActionsFromProps(turnClick);
				if(IsPlayerOne())
				{
					P2Submitted = true;
					if(BothPlayersHaveSubmitted())
					{
						turnNumber++;
						CalcMoves();
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
				//oppCharacers = LoadCharactersFromProps(turnClick);
				SetTeams(oppHT);
				
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
				ExecuteMoves(turnClick);
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
				//if (this.CurrentRoom.Players.Count == 1 && !this.GameWasAbandoned)
				{
					this.CurrentRoom.IsOpen = true;
					this.CurrentRoom.IsVisible = true;
				}
				break;
		}
		Debug.Log(photonEvent.Code.ToString());
	}
//	IEnumerator WaitForBothPlayers()
//	{
//		while(!(P1Submitted && P2Submitted))
//		{
//			
//			yield return new WaitForSeconds(0.2f);
//		}
//	}

	public void LoadBoardFromProperties(bool calledByEvent)
	{	
		Hashtable roomProps = this.CurrentRoom.CustomProperties;
		Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));

		if (roomProps.Count == 0)
		{
			// we are in a fresh room with no saved board.
			board.Generate(21,11);
			//gui.UIState = gui.UISP;
			this.SaveBoardToProperties();
			Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));
		}
		
		// we are in a game that has props (a board). read those (as update or as init, depending on calledByEvent)
		bool success = board.SetBoardByCustomProperties(roomProps, calledByEvent);
		if (!success)
		{
			Debug.LogError("Not loaded board from props?");
		}
		this.myTeam = Grid_Setup.Instance.Teams [(int)team];

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
	
	void SetTeams(Hashtable HT)
	{
		if(BothPlayersHaveSubmitted())
		{
			oppCharacers = LoadCharactersFromProps(HT);
			P1Submitted = false;
			P2Submitted = false;
		}
	}
	
	public void SaveBoardToProperties()
	{
		Hashtable boardProps = board.GetBoardAsCustomProperties();
		//boardProps.Add("pt", this.PlayerIdToMakeThisTurn);  // "pt" is for "player turn" and contains the ID/actorNumber of the player who's turn it is
		boardProps.Add("t#", this.TurnNumber);
		boardProps.Add("tx#", board.Width);
		boardProps.Add("tz#", board.Length);
		
		//boardProps.Add(GetPlayerPointsPropKey(this.LocalPlayer.ID), this.MyPoints); // we always only save "our" points. this will not affect the opponent's score.
		bool webForwardToPush = false;

		//Debug.Log(string.Format("saved board to room-props {0}", SupportClass.DictionaryToString(boardProps)));
		this.OpSetCustomPropertiesOfRoom(boardProps, webForwardToPush);
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

	public Player Opponent
	{
		get
		{
			Player opp = this.LocalPlayer.GetNext();
			//Debug.Log("you: " + this.LocalPlayer.ToString() + " other: " + opp.ToString());
			return opp;
		}
	}
	public void CalcMoves()
	{
		Hashtable MoveSet = new Hashtable();

//		for (int i = 0; i<myActions.Length; i++) 
//		{
//			if(myActions[i]!=null)
//			{
//				for (int j = 0; j<oppActions.Length; j++) 
//				{
//					if(oppActions[j]!=null)
//					{
//						if(myActions[i].cTo==oppActions[j].cTo)
//						{
//							if(myActions[i].action == PlayerAction.Actions.Move && oppActions[j].action== PlayerAction.Actions.Move)
//							{
//								float m = Random.Range(0,5), o = Random.Range(0,5);
//								Debug.Log(m+", "+o);
//								if(m>o)
//								{
//									MoveSet[(i).ToString()] = myActions[i].GetActionProp();
//								}else 
//								{
//									MoveSet[(i+myActions.Length).ToString()] = oppActions[i].GetActionProp();
//								}
//							}
//						}else{	
//						}
//						MoveSet[(i+myActions.Length).ToString()] = oppActions[i].GetActionProp();
//					}
//				}
//				MoveSet[(i).ToString()] = myActions[i].GetActionProp();
//			}
//		}
		for (int i = 0; i<oppActions.Length; i++) 
		{
			if(oppActions[i]!=null)
			{
				MoveSet[(i).ToString()] = oppActions[i].GetActionProp();
			}
			
		}
		for (int i = 0; i<myActions.Length; i++) 
		{
			if(myActions[i]!=null)
			{
				MoveSet[(i+oppActions.Length).ToString()] = myActions[i].GetActionProp();
			}

		}
		ExecuteMoves(MoveSet);

		this.loadBalancingPeer.OpRaiseEvent(Execute, MoveSet, true, null);
		//new RaiseEventOptions{Receivers = ReceiverGroup.All }
	
	}
	void ExecuteMoves(Hashtable moves)
	{
		PlayerAction[] acts = LoadActionsFromProps (moves);
		List<FSM_Character> affectedChars = new List<FSM_Character>();
		for (int h = 0; h < acts.Length; h++) 
		{
			if(acts[h]!=null)
			{
				Debug.Log(acts[h].action.ToString());
				if(!affectedChars.Contains(acts[h].iCh))
				{
					affectedChars.Add(acts[h].iCh);
				}
				acts[h].iCh.SetPlayerAction(acts[h]);
			}
		}
		foreach(FSM_Character c in affectedChars)
		{
			c.StartCoroutine("ExecuteActions");
		}
		ClearActions();
		P1Submitted = false;
		P2Submitted = false;
	}
}
