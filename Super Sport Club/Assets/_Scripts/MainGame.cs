using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class MainGame : MonoBehaviour 
{
	public static MainGame Instance{
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
	public bool bOnline;
	public float timeSinceService;
	public byte MaxActions = 5;
	public bool bTurnDone;
	public int team;
	public Team[] Teams;
	public int TeamSize{ get{return teamSize;}}
	public int TurnNumber{get{return turnNumber;}}
	public byte ActionsLeft{get{return (byte)(MaxActions - actionCount);}}
	[SerializeField] private int teamSize = 5;
	[SerializeField] string AppId;// set in inspector. this is called when the client loaded and is ready to start
	[SerializeField] float serviceInterval = 1;
	[SerializeField] private CharacterData[] positionData;
	[SerializeField] private GameObject charFab = null;
	[SerializeField] private Color[] TeamColors =  {Color.black, Color.white};
	int[] score;
	Team myTeam;
	PlayerAction[] myActions, oppActions;
	bool P1Submitted, P2Submitted, connectInProgress;
	int turnNumber;//, teamSize = 5;
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
		this.GameClientInstance.board = board;
		this.GameClientInstance.mainGame = this;
		Application.runInBackground = true;
		CustomTypes.Register();
		myActions = new PlayerAction[MaxActions];
		oppActions = new PlayerAction[MaxActions];
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
			myActions[actionCount] = act;
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
					Teams [t].mates [c].charData = positionData [c];
					newGuy.SetActive (false);
				}
			}
	}


	public UnitController GetCharacter(int Team, int index)
	{
		return Teams [Team].mates [index];
	}
	public void SetCharacter(int team,int index, Vector3 location)
	{
		if(team<Teams.Length && index<teamSize)
		{
			Teams[team].mates[index].gameObject.SetActive(true);
			Teams[team].mates[index].MoveTransform(board.GetCellByLocation(location).Location);
		}
	}
	public void LoadCharactersFromProps(Hashtable ht)
	{
		for(int i = 0;i<ht.Count;i++)
		{
			Hashtable hash = ht[i.ToString()]as Hashtable;
			int team = (int)hash["Team"];
			Vector3 loc = (Vector3)hash["Location"];
			this.SetCharacter(team, i, loc);
		}
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
		for(int c= 0;c<myActions.Length;c++)
		{
			if(myActions[c]!=null)
			{
				myActions[c] = null;
			}
		}
		board.TurnOffHiglightedAdjacent ();
		actionCount = 0;
		foreach(UnitController c in Teams[(int)GameClientInstance.team].mates)
		{
			c.ClearActions();
		}
	}

	public void Connect()
	{
		connectInProgress = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors
	}

	public void NewOnlineGame()
	{
		if (connectInProgress) 
		{
			GameClientInstance.OpJoinRandomRoom (null, 0);

		} else {
			GameClientInstance.Disconnect ();
			connectInProgress = GameClientInstance.ConnectToRegionMaster("us"); 
			Debug.Log ("I Can't Even");
		}
	}

	public void NewLocalGame()
	{
		CreateTeams();
		board.Generate();
	}

	public void SubmitTeam()
	{
		
	}

	void NextTurn()
	{
		ClearActions();
		UnityEventManager.TriggerEvent ("NextTurn");
		turnNumber++;
		P1Submitted = false;
		P2Submitted = false;
	}

	public void EndTurn()
	{
		if(bOnline)
		this.GameClientInstance.EndTurnEvent(myActions);
		else CalcMoves();
	}

	void CalcMoves()
	{
	//The idea is to sort each Player's actions to figure out what will actually happen vs plans. 
	//Compiled list od acts is then acted upon by both Players
		Hashtable MoveSet = new Hashtable();
		//List<Cell> targetedCells = new List<Cell> ();

		for (int i = 0; i<oppActions.Length; i++) 
		{
			if(oppActions[i]!=null)
			{
				//targetedCells.Add (oppActions[i]);
				MoveSet[(i).ToString()] = oppActions[i].GetActionProp();
			}
		}
		for (int i = 0; i<myActions.Length; i++) 
		{
			if(myActions[i]!=null)
			{
				//int conflict = targetedCells.BinarySearch(myActions[i].cTo);
				MoveSet[(i+oppActions.Length).ToString()] = myActions[i].GetActionProp();
			}
		}
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
				Debug.Log(acts[h].action.ToString());
				if(!affectedChars.Contains(acts[h].iCh))
				{
					affectedChars.Add(acts[h].iCh);
				}
				affectedChars[h].SetPlayerAction(acts[h]);
			}
		}
		foreach(UnitController c in affectedChars)
		{
			c.StartCoroutine("ExecuteActions");
		}

		NextTurn ();
	}
}

