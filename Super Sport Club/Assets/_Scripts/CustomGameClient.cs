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
	Message message;
	byte actionCount = 0;
	public byte MaxActions = 5;
	public bool bTurnDone;
	public const byte EndTurn = 1;
	public const byte Execute = 2;
	public const string PropTurn = "turn";
	public const string PropNames = "names";
	public Grid_Setup board;
	PlayerAction[] myActions, oppActions;
	bool P1Submitted, P2Submitted;
	public FSM_Character[] characters;
	int TurnNumber;
	public GUIController gui;

	public CustomGameClient()
	{
		myActions = new PlayerAction[MaxActions];

	}
	public void EndTurnEvent()
	{
		Hashtable content = GetActionsAsProps (myActions);
		if (IsPlayerOne()) 
		{
			P1Submitted = true;
		}else{P2Submitted=true;}

		this.loadBalancingPeer.OpRaiseEvent(EndTurn, content, true, null);
	}
	bool IsPlayerOne()
	{
		return this.LocalPlayer.IsMasterClient;
	}
	bool BothPlayersHaveSubmitted()
	{
		return P1Submitted && P2Submitted;
	}

	public void SetPlayerAction(PlayerAction.Actions act, FSM_Character character, Cell targetCell)
	{
		if(actionCount<MaxActions&&character.actionCount<character.maxActions)
		{
			myActions[actionCount] = new PlayerAction(act, character, targetCell);
			actionCount += 1;
			if(act == PlayerAction.Actions.Move)
			{
				character.SetTarget(targetCell);
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

	PlayerAction[] LoadActionsFromProps(Hashtable ht)
	{
		PlayerAction[] actions = new PlayerAction[ht.Count];
		for(int i = 0;i<ht.Count;i++)
		{
			if(ht[i.ToString()]!=null)
			{
				Hashtable ion = ht[i.ToString()]as Hashtable;
				PlayerAction.Actions act = (PlayerAction.Actions)ion["Act"];
				FSM_Character ich = characters[ (int)ion["iCharacter"]];
				Cell cell =	board.cells[(int)ion["tCell"]];
				actions[i] = new PlayerAction(act,ich,cell);
			}
		}
		return actions;
	}

	public void ClearActions()
	{
		List<FSM_Character> affectedChars = new List<FSM_Character>();
		for(int c= 0;c<myActions.Length;c++)
		{
			if(myActions[c]!=null)
			{
				if(!affectedChars.Contains(myActions[c].iCh))
				{
					affectedChars.Add(myActions[c].iCh);
				}
				myActions[c] = null;
			}
		}
		foreach(FSM_Character c in affectedChars)
		{
			c.ClearActions();
		}
		board.TurnOffHiglighted ();
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
			if(IsPlayerOne())
			{
				P2Submitted = true;
			}else{P1Submitted=true;}
			object content = photonEvent.Parameters[ParameterCode.CustomEventContent];
			Hashtable turnClick = content as Hashtable;
			oppActions = LoadActionsFromProps(turnClick);
			if( BothPlayersHaveSubmitted())
			{
				CalcMoves();
				Debug.Log("Go and get yourself a good fucking");
			}

			break;
		}
		case EventCode.PropertiesChanged:
			//Debug.Log("Got Properties via Event. Update board by room props.");
			this.LoadBoardFromProperties(true);
			//this.board.ShowFlippedTiles();
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

	public void LoadBoardFromProperties(bool calledByEvent)
	{	
		Hashtable roomProps = this.CurrentRoom.CustomProperties;
		Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));

		if (roomProps.Count == 0)
		{
			// we are in a fresh room with no saved board.
			board.Generate(20,10);
			this.SaveBoardToProperties();
			Debug.Log(string.Format("Board Properties: {0}", SupportClass.DictionaryToString(roomProps)));
		}
		
		// we are in a game that has props (a board). read those (as update or as init, depending on calledByEvent)
		bool success = board.SetBoardByCustomProperties(roomProps, calledByEvent);
		if (!success)
		{
			Debug.LogError("Not loaded board from props?");
		}
		

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
	
	public void SaveBoardToProperties()
	{
		Hashtable boardProps = board.GetBoardAsCustomProperties();
		//boardProps.Add("pt", this.PlayerIdToMakeThisTurn);  // "pt" is for "player turn" and contains the ID/actorNumber of the player who's turn it is
		//boardProps.Add("t#", this.TurnNumber);
		boardProps.Add("tx#", board.width);
		boardProps.Add("tz#", board.length);
		foreach(FSM_Character c in characters)
		{
			boardProps.Add("character#"+c.id,c.GetCharacterAsProp());
		}
		//boardProps.Add(GetPlayerPointsPropKey(this.LocalPlayer.ID), this.MyPoints); // we always only save "our" points. this will not affect the opponent's score.
		
		
		// our turn will be over if 2 tiles are clicked/flipped but not the same. in that case, we update the other player if inactive
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
		List<FSM_Character> affectedChars = new List<FSM_Character>();
		Hashtable MoveSet = new Hashtable();
		for (int i = 0; i<myActions.Length; i++) 
		{
			if(myActions[i]!=null)
			{
				MoveSet.Add(i.ToString(),myActions[i].GetActionProp());
			}
			
		}
		for (int i = 0; i<oppActions.Length; i++) 
		{
			if(oppActions[i]!=null)
			{
				MoveSet.Add((i+myActions.Length).ToString(),oppActions[i].GetActionProp());
			}
			
		}
		
//		for(int h = 0; h < myActions.Length; h++)
//		{
//			if(myActions[h]!=null)
//			{
//				if(!affectedChars.Contains(myActions[h].iCh))
//				{
//					affectedChars.Add(myActions[h].iCh);
//				}
//			}else break;
//			affected["charcter#"] = affectedChars[h].GetCharacterAsProp();
//		}
		
//		foreach(FSM_Character c in affectedChars)
//		{
//			c.StartCoroutine("ExecuteActions");
//		}
//		Cell tCell = new Cell();
//		for(int h = 0; h < myActions.Length; h++)
//		{
//			tCell = myActions[h].cTo;
//			for(int j = 0; j < oppActions.Length; j++)
//			{
//				if(tCell == oppActions[j].cTo)
//				{
//				}
//			}
//		}

		this.loadBalancingPeer.OpRaiseEvent(Execute, MoveSet, true, new RaiseEventOptions{Receivers = ReceiverGroup.All });
		
		//yield return null;
		
		/*
		Cell tCell = new Cell();
		for(int h = 0; h < a.Length; h++)
		{
			tCell = a[h].cTo;
			for(int j = 0; j < b.Length; j++)
			{
				if(tCell==b[j].cTo)
				{
				}
			}
		}
		*/
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
	}
}
