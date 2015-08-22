using System.Collections;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
	public byte MaxActions = 5;
	PlayerAction[] acts;

	[SerializeField] Image panelFab;
	[SerializeField] Button buttFab;
	[SerializeField] Canvas can;
	public float offsetX,offsetY;
	public static GameObject panel;

	Message message;
	public static bool bSelection;
	//public static GameObject CurrentSelectedObj;
	public FSM_Character CurrentSelectedChar
	{
		get{return characters[currentID];}
	}

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

	public void ClearActions()
	{
		List<FSM_Character> affectedChars = new List<FSM_Character>();
		for(int c= 0;c<acts.Length;c++)
		{
			if(acts[c]!=null)
			{
				if(!affectedChars.Contains(acts[c].iCh))
				{
					affectedChars.Add(acts[c].iCh);
				}
				acts[c] = null;
			}
		}
		foreach(FSM_Character c in affectedChars)
		{
			c.ClearActions();
		}
		board.TurnOffHiglighted ();
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
		if(actionCount<MaxActions&&character.actionCount<character.maxActions)
		{
			acts[actionCount] = new PlayerAction(act, character, targetCell);
			character.SetPlayerAction(acts[actionCount]);
			actionCount += 1;
		}
	}
	void MovementClick(int index)
	{
		SetPlayerAction(PlayerAction.Actions.Move, CurrentSelectedChar, board.cells[index]);
		if (CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount > 0) 
		{
			//board.HighlightAdjacent (false, CurrentSelectedChar.OccupiedCell.id, CurrentSelectedChar.maxActions);
			board.HighlightAdjacent (true, index, CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount );
		} else {
			board.TurnOffHiglighted();
		}

	}
	void CreateButtonPanel(int index)
	{
		if (panel!=null)
		{
			Destroy(panel.gameObject);
		}
		Vector3 loc = Camera.main.WorldToScreenPoint(board.cells[index].GetLocation());
		loc += new Vector3(offsetX,offsetY,0f);
		panel = Instantiate(panelFab.gameObject, loc, Quaternion.identity)as GameObject;
		panel.transform.SetParent(can.transform,false);
		panel.transform.SetAsLastSibling();

		if ((CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount > 0)) 
		{
			if(CurrentSelectedChar.actionCount > 0) 
			{
				Button clearButton = Instantiate (buttFab) as Button;
				clearButton.transform.SetParent (panel.transform, false);
				clearButton.GetComponentInChildren<Text> ().text = "Clear";
				clearButton.onClick.AddListener (() => 
				{ 
					CurrentSelectedChar.ClearActions();
					board.TurnOffHiglighted();
					Destroy (panel.gameObject);
				});
			}

			Button moveButton = Instantiate (buttFab) as Button;
			moveButton.transform.SetParent (panel.transform, false);
			moveButton.GetComponentInChildren<Text> ().text = "Move";
			moveButton.onClick.AddListener (() => 
			{ 	
				if(CurrentSelectedChar.targetCount>0)
				{
					board.HighlightAdjacent (true, CurrentSelectedChar.LastTargetCell.id, CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount);
				}
				else board.HighlightAdjacent (true, index, CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount);
				
				Destroy (panel.gameObject);
			});
		
			if (CurrentSelectedChar.hasBall) {
				Button passButton = Instantiate (buttFab) as Button;
				passButton.transform.SetParent (panel.transform, false);
				passButton.GetComponentInChildren<Text> ().text = "Pass";
				passButton.onClick.AddListener (() => 
				{ 
					SetPlayerAction (PlayerAction.Actions.Pass, CurrentSelectedChar, board.cells [index]);
					Destroy (panel.gameObject);
				});	
			}

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
							{
								CurrentSelectedChar.Highlight(false);
								board.TurnOffHiglighted();
							}

							currentID = id;
							CurrentSelectedChar.Highlight(true);
							CurrentSelectedChar.OccupiedCell = board.cells[CurrentSelectedChar.RaycastToGround()];//this should be done when placing characters
						}
						CreateButtonPanel(CurrentSelectedChar.OccupiedCell.id);
						break;

					}
					case "Field":
					{
						if(!EventSystem.current.IsPointerOverGameObject())
						{
							if(bSelection)
							{
								int index  = hit.transform.GetSiblingIndex();
								MovementClick( index);
							}
						}
						break;
					}
				}
			}
			//p = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			
		}
		if (Input.GetMouseButtonUp (1)) 
		{
			//actionCount = 0; //make a button
		}
	}
}
