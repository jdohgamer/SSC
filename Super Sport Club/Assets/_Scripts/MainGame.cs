﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class MainGame : MonoBehaviour 
{
	public static MainGame Instance
	{
		get
		{
			if (!instance)
			{
				instance = FindObjectOfType (typeof (MainGame)) as MainGame;

				if (!instance)
				{
					Debug.LogError ("There needs to be one active MainGame script on a GameObject in your scene.");
				}
				else
				{
					instance.StartGame(); 
				}
			}

			return instance;
		}
	}
	public bool bOnline, bDev;
	public float timeSinceService;
	public byte MaxActions = 5;
	public Team[] Teams;
	public int TeamSize{ get{return teamSize;}}
	public int TurnNumber{get{return turnNumber;}}
	public byte ActionsLeft{get{return (byte)(MaxActions - actionCount);}}
	public Team CurrentTeam{get{return Teams[currentTeam];}}
	public int CurrentTeamNum{
		get
		{
			if(bDev)
			return currentTeam;
			else return teamNum;
		}
		set
		{
			if(bDev)
			currentTeam = value;
			else teamNum = value;
		}
	}
	public PlayerAction[] CurrentActionSet{get{return characterActions[currentTeam];}}
	[SerializeField] private int teamSize = 5, currentTeam;
	[SerializeField] string AppId;// set in inspector. this is called when the client loaded and is ready to start
	[SerializeField] float serviceInterval = 1;
	[SerializeField] private CharacterData[] positionData;
	[SerializeField] private GameObject charFab = null;
	[SerializeField] private Color[] TeamColors =  {Color.black, Color.white};
	int[] score;
	PlayerAction[][] characterActions;

	bool P1Submitted, P2Submitted, connectInProgress;
	int turnNumber, teamNum;//, teamSize = 5;
	byte actionCount = 0;
	private CustomGameClient GameClientInstance;
	private GUIController gui;
	private Grid_Setup board;
	private static MainGame instance;


	public void StartGame()
	{
		this.gui = GetComponent<GUIController>();
		this.board = GetComponent<Grid_Setup>();
		Grid_Setup.Instance = this.board;
		this.GameClientInstance  = new CustomGameClient();
		CustomGameClient.ClientInstance = this.GameClientInstance;
		this.GameClientInstance.AppId = AppId;  // edit this!
		this.GameClientInstance.mainGame = this;
		Application.runInBackground = true;
		CustomTypes.Register();
		characterActions = new PlayerAction[][]{new PlayerAction[MaxActions], new PlayerAction[MaxActions]};
		//oppCharacers = new FSM_Character[teamSize];
		score = new int[2];

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

	public void SetPlayerAction(PlayerAction act)
	{
		if(actionCount<MaxActions&&act.iCh.actionCount<act.iCh.maxActions)
		{
			CurrentActionSet[actionCount] = act;
			actionCount += 1;
			if(act.action == PlayerAction.Actions.Move)// preview waypoints
			{
				act.iCh.SetMoveTarget(act.cTo);
			}
			if(act.action == PlayerAction.Actions.Pass || act.action == PlayerAction.Actions.Shoot || act.action == PlayerAction.Actions.Cross)
			{
				act.iCh.SetPassTarget(act.cTo);
			}
		}
	}

	public void SetOtherTeamActions(Hashtable ht)
	{
		characterActions[(currentTeam+1)%2] = LoadActionsFromProps(ht);
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

	public void ClearActions()
	{
		for(int c= 0;c<CurrentActionSet.Length;c++)
		{
			if(CurrentActionSet[c]!=null)
			{
				CurrentActionSet[c] = null;
			}
		}
		board.TurnOffHiglightedAdjacent ();
		actionCount = 0;
//		foreach(UnitController c in CurrentTeam.mates)
//		{
//			c.ClearActions();
//		}
	}

	public bool IsShotOnGoal(int tNum, Vector3 spot)
	{
		return Teams[tNum].IsVectorInGoal (spot);
		
	}

		public int TeamScore(int team)
	{
		if (team > 1 || team<0) 
		{
			return -1;
		}
		return score [team];
	}
	public void ScorePoint(int team)
	{
		if (team > 1 || team<0) 
		{
			return;
		}
		score [team] += 1;
	}

	void CreateTeams()
	{
		Teams = new Team[2];
		for(int t = 0; t<2 ; t++)
			{
				//bool teamOne = t == 0;
				Vector3 goal = t == 0 ? board.TeamOneGoal : board.TeamTwoGoal;
				Teams [t] = new Team (t, TeamColors[t], teamSize, goal, board.GoalSize);
				//Quaternion face = teamOne ? Quaternion.LookRotation(Vector3.right):Quaternion.LookRotation(-Vector3.right) ;
				for(int c = 0; c <teamSize; c++)
				{
					GameObject newGuy = Instantiate(charFab,Vector3.zero + new Vector3((float)t,0.2f,(float)c),Quaternion.identity) as GameObject;
					Teams [t].AddMate(newGuy.GetComponent<UnitController>());
					Teams [t].mates [c].team = t;
					Teams [t].mates [c].charData = positionData [c];
					newGuy.SetActive (false);
				}
			}
	}


	public UnitController GetCharacter(int Team, int index)
	{
		return Teams [Team].mates [index];
	}

	bool TeamActive(Team team)
	{
		int all=0;
		for(int i = 0; i<teamSize; i++)
		{
			if(team.mates[i].BActive)
			{
				all++;
			}
		}
		return all == teamSize;
	}

	public void SetCharacterPosition(int team,int index, Vector3 location)
	{
		if(team<Teams.Length && index<teamSize)
		{
			Teams[team].mates[index].BActive = true;
			Teams[team].mates[index].MoveTransform(board.GetCellByLocation(location).Location);
			board.GetCellByLocation(location).UnitOccupier = Teams[team].mates[index];
			//have a counter or signal here
		}
	}

	public void LoadCharactersFromProps(Hashtable ht)
	{
		for(int i = 0;i<ht.Count;i++)
		{
			Hashtable hash = ht[i.ToString()]as Hashtable;
			int team = (int)hash["Team"];
			Vector3 loc = (Vector3)hash["Location"];
			this.SetCharacterPosition(team, i, loc);
		}
	}

	public void Connect()
	{
		connectInProgress = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors
		//StartCoroutine("NewOnlineGame");
	}

	public IEnumerator NewOnlineGame()
	{
		yield return new WaitForSeconds(4);

			if (connectInProgress) 
			{
				GameClientInstance.OpJoinRandomRoom (null, 0);
				bOnline = true;

			} else {
				connectInProgress = GameClientInstance.ConnectToRegionMaster("us"); 
				Debug.Log ("I Can't Even");
			}
	}

	public void NewGame()
	{
		CreateTeams();
		board.Generate();
		gui.UIState.ToSetPiece();
	}

	public void SubmitTeam()
	{
		if(TeamActive(CurrentTeam))
		{
			if(bOnline)
			this.GameClientInstance.SubmitTeamEvent(CurrentTeam);

			if(bDev&&!(TeamActive(Teams[0])&&TeamActive(Teams[1])))
			{
				currentTeam = (currentTeam+1)%2;
				gui.UIState.ToGameHUD();
				gui.UIState.ToSetPiece();
			}else{
				gui.UIState.ToGameHUD();
				NextTurn();
			}


		}else {
			Debug.Log ("You still have players to place");
		}
	}

	void NextTurn()
	{
		ClearActions();
		UnityEventManager.TriggerEvent ("NextTurn");
		turnNumber++;
//		P1Submitted = false;
//		P2Submitted = false;
	}

	public void EndTurn()
	{
		if(bOnline)
		this.GameClientInstance.EndTurnEvent(CurrentActionSet);
		else {CalcMoves();}
	}

	void CalcMoves()
	{
	//The idea is to sort each Player's actions to figure out what will actually happen vs plans. 
	//Compiled list od acts is then acted upon by both Players
		Hashtable MoveSet = new Hashtable();
		//List<Cell> targetedCells = new List<Cell> ();
		Debug.Log ("Calculating");
		for(int j = 0;j<characterActions.Length; j++)
		{
			for (int i = 0; i<characterActions[j].Length; i++) 
			{
				if(characterActions[j][i]!=null)
				{
					//targetedCells.Add (oppActions[i]);
					MoveSet[(i).ToString()] = characterActions[j][i].GetActionProp();
				}
			}
		}
//		for (int i = 0; i<characterActions[1].Length; i++) 
//		{
//			if(characterActions[1][i]!=null)
//			{
//				//targetedCells.Add (oppActions[i]);
//				MoveSet[(i).ToString()] = CurrentActionSet[i].GetActionProp();
//			}
//		}
//		for (int i = 0; i<characterActions[0].Length; i++) 
//		{
//			if(characterActions[0][i]!=null)
//			{
//				//int conflict = targetedCells.BinarySearch(myActions[i].cTo);
//				MoveSet[(i+CurrentActionSet.Length).ToString()] = CurrentActionSet[i].GetActionProp();
//			}
//		}
		ExecuteMoves(MoveSet);

	}

	void ExecuteMoves(Hashtable moves)
	{
		PlayerAction[] acts = LoadActionsFromProps (moves);
		List<UnitController> affectedChars = new List<UnitController>();
		for (int h = 0; h < acts.Length; h++) 
		{
			if(acts[h]!=null)
			{
				UnitController unit = acts[h].iCh;
				Debug.Log(acts[h].action.ToString()+", "+acts[h].cTo.Location);
				if(!affectedChars.Contains(unit))
				{
					affectedChars.Add(unit);
				}
				unit.SetPlayerAction(acts[h]);
			}
		}
		foreach(UnitController c in affectedChars)
		{
			c.StartCoroutine("ExecuteActions");
		}

		NextTurn ();
	}
}
