﻿using System.Collections;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameController : MonoBehaviour 
{
	public static GameController Instance = null;
	Grid_Setup board;
	public Grid_Setup Grid{get{return board;}}
	public int length, width, currentID = -1;
	bool bWaitingOnPlayers;
	//public Cell[] cells;
	CustomGameClient gameClientInstance;
	public CustomGameClient GameClientInstance{get{return gameClientInstance;}}
	Player[] players ;
	FSM_Character[] characters;
	public enum Teams{TeamOne,TeamTwo}
	byte actionCount = 0;
	PlayerAction[] acts;

	Message message;
	public static bool bSelection;
	//public static GameObject CurrentSelectedObj;
	public FSM_Character CurrentSelectedChar
	{
		get{return characters[currentID];}
	}
	public GameObject destPin;
	GameObject newPin, oldPin;

	[SerializeField] LayerMask mask;

	void Awake()
	{
		if (Instance == null)
			Instance = this;
		else if (Instance != this)
			Destroy(gameObject); 

		this.board = GetComponent<Grid_Setup>();
		this.gameClientInstance = new CustomGameClient();
		this.gameClientInstance.board = board;
		Application.runInBackground = true;
		CustomTypes.Register();
		// "eu" is the European region's token
		bool connectInProcess = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors
	}
	void Start () 
	{
		acts = new PlayerAction[5];
		GameObject[] charObjs = GameObject.FindGameObjectsWithTag("Player");
		characters = new FSM_Character[charObjs.Length];
		for(int i = 0; i< charObjs.Length;i++)
		{
			FSM_Character temp = charObjs[i].GetComponent<FSM_Character>();
			if(temp!= null)
			{
				characters[i] = temp;
				characters[i].id = i;
			}
		}
		message = new Message ();
	}
	public void EndTurn()
	{
		bSelection = false;
		//Destroy(oldPin);
		if(acts.Length>0)
		{
			ExecuteMoves();
		}
	}

	void Update()
	{
		if(!HaveBothPlayersSubmitted())
		{
		}

	}
	bool HaveBothPlayersSubmitted()
	{
		return true;
	}

	void ClearActions()
	{
		for(int c= 0;c<acts.Length;c++)
		{
			acts[c] = null;
		}
		actionCount = 0;
	}

	void ExecuteMoves()
	{
		List<FSM_Character> affectedChars = new List<FSM_Character>();
		for(int h = 0; h < acts.Length; h++)
		{
			if(acts[h]!=null)
			{
				if(!affectedChars.Contains(acts[h].iCh))
				{
					affectedChars.Add(acts[h].iCh);
				}

				acts[h].iCh.SetPlayerAction(acts[h]);
//				
			}else break;
		}
		foreach(FSM_Character c in affectedChars)
		{
			c.StartCoroutine("ExecuteActions");
		}
		ClearActions();
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

	public void SetPlayerAction(PlayerAction.Actions act, FSM_Character character, Cell targetCell)
	{
		if(actionCount<5)
		{
			acts[actionCount] = new PlayerAction(act, character, targetCell);
			actionCount += 1;
		}
	}
	public void FixedUpdate() 
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			message.Clear();
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
			if (Physics.Raycast (ray, out hit, 100f, mask)) 
			{
				switch(hit.transform.tag)
				{
					case "Player":
					{
						bSelection = true;
						Debug.Log ("Dick");

						int id = hit.transform.gameObject.GetComponent<FSM_Character>().id;
						if(id!= currentID)
						{
							if(currentID!=-1)
							CurrentSelectedChar.Highlight(false);

							currentID = id;
							CurrentSelectedChar.Highlight(true);
							oldPin = newPin;
							newPin = null;
							CurrentSelectedChar.OccupiedCell = board.cells[CurrentSelectedChar.RaycastToGround()];//this should be done when placing characters
						}
						board.HighlightAdjacent(CurrentSelectedChar.OccupiedCell.id, CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount);
							
						break;
					}
					case "Field":
					{
						if(bSelection)
						{
							int index  = hit.transform.GetSiblingIndex();
							

							SetPlayerAction(PlayerAction.Actions.Move, CurrentSelectedChar, board.cells[index]);

							if(newPin != null)
							{
								Destroy(newPin);
							}
							newPin = Instantiate(destPin,board.cells[index].GetLocation(),Quaternion.identity) as GameObject;
							
						}
						break;
					}
				}
			}
			//p = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			
		}
		if (Input.GetMouseButtonUp (1)) 
		{
			actionCount = 0;
		}
	}
}