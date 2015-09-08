using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GUIController: MonoBehaviour
{
	public static bool bSelection;
	public static GameObject panel, meter;
	public float offsetX,offsetY;
	public float serviceInterval = 1;
	public float timeSinceService;
	public string AppId;            // set in inspector. this is called when the client loaded and is ready to start
	public FSM_Character CurrentSelectedChar
	{
		get{return characters[currentID];}
	}
	[SerializeField] LayerMask mask;
	[SerializeField] Image panelFab;
	[SerializeField] SpriteRenderer meterFab;
	[SerializeField] Button buttFab;
	[SerializeField] Canvas UIcan;
	int currentID = -1;
	FSM_Character[] characters;
	Grid_Setup board;
	bool isPassing, isMoving;
	Vector3 idealPassDir, actualPassDir, simpleDir, offsetDir;

	private CustomGameClient GameClientInstance;

	void Awake()
	{
		this.board = GetComponent<Grid_Setup>();
		this.GameClientInstance  = new CustomGameClient();
		this.GameClientInstance.AppId = AppId;  // edit this!
		this.GameClientInstance.board = board;
		this.GameClientInstance.gui = this;

		Application.runInBackground = true;
		CustomTypes.Register();
		bool connectInProcess = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors

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
		this.GameClientInstance.characters = this.characters;
		this.board.characters = this.characters;
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

	public void FixedUpdate() 
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
			if (Physics.Raycast (ray, out hit, 100f, mask)) 
			{
				switch(hit.transform.tag)
				{
					case "Player":
					{
						bSelection = true;
						
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
						CurrentSelectedChar.OccupiedCell = board.cells[CurrentSelectedChar.RaycastToGround()];
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
								if(isPassing)
								{
									CreatePlayerMeter(index);
									//PassClick(index);
								}else if(isMoving)
								{
									MovementClick( index);
								}
							}
						}
						break;
					}
				}
			}	
		}
		if (Input.GetMouseButtonUp (1)) 
		{
			DeselectCharacter();
		}
	}

	void DeselectCharacter()
	{
		if(currentID!=-1)
		{
			isMoving = false;
			isPassing = false;
			CurrentSelectedChar.Highlight(false);
			board.TurnOffHiglighted();
			currentID = -1;
		}
	}

	void MovementClick(int index)
	{
		int actsLeft = CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount;
		if (actsLeft > 0) 
		{
			GameClientInstance.SetPlayerAction(new PlayerAction(PlayerAction.Actions.Move, CurrentSelectedChar, board.cells[index]));
			actsLeft--;
			if (actsLeft <= 0) 
			{
				board.TurnOffHiglighted();
				isMoving = false;
			}else{
				board.HighlightAdjacent (true, index, actsLeft);
			}
		} 
	}	

	void PassClick(int index)
	{
		if (CurrentSelectedChar.maxActions -CurrentSelectedChar.actionCount > 0) 
		{
			GameClientInstance.SetPlayerAction(new PlayerAction(PlayerAction.Actions.Pass, CurrentSelectedChar, board.cells[index], CurrentSelectedChar.OccupiedCell));
			board.TurnOffHiglighted();
			isPassing = false;
		}
	}

	void CreatePlayerMeter(int index)
	{
		Vector3 CharacterPosition = CurrentSelectedChar.transform.position;
		idealPassDir = board.cells[index].GetLocation() - CharacterPosition;
		idealPassDir.y = 0;
		
		float ang = Vector3.Angle(Vector3.forward,idealPassDir.normalized);
		Debug.Log(ang);
		if(ang<45)
		{
			simpleDir = Vector3.forward;
			offsetDir = Vector3.down;
		}
		if(ang>=45&&ang<=135)
		{
			if(idealPassDir.x<0)
			{
				simpleDir = Vector3.left;
			}else simpleDir = Vector3.right;
			offsetDir = -simpleDir;
		}
		if(ang>135)
		{
			simpleDir = Vector3.back;
			offsetDir = Vector3.up;
		}
		if (meter!=null)
		{
			Destroy(meter.gameObject);
		}
		Vector3 loc = CharacterPosition;
		meter = Instantiate(meterFab.gameObject, loc, Quaternion.LookRotation(simpleDir))as GameObject;
		meter.GetComponent<Gauge>().SetIdeal(idealPassDir);
		
		if (panel!=null) 
		{
			Destroy(panel.gameObject);
		}
		
		loc= Camera.main.WorldToScreenPoint(CharacterPosition);
		loc += offsetDir*50;
		panel = Instantiate(panelFab.gameObject, loc, Quaternion.identity)as GameObject;
		panel.transform.SetParent(UIcan.transform,false);
		panel.transform.SetAsLastSibling(); 
		
		Button clearButton = Instantiate (buttFab) as Button;
		clearButton.transform.SetParent (panel.transform, false);
		clearButton.GetComponentInChildren<Text> ().text = "Kick";
		clearButton.onClick.AddListener (() => 
		{ 
			Vector3 kick = meter.GetComponent<Gauge>().StopBounce();
			Vector3 cellPos = CharacterPosition+kick* idealPassDir.magnitude;
			Ray ray = new Ray(cellPos, Vector3.down);
			RaycastHit hit;
			int cell;
			if (Physics.Raycast (ray, out hit, 10f, mask)) 
			{
				if(hit.transform.tag == "Field")
				{
					cell = hit.transform.GetSiblingIndex();
					PassClick(cell);
				}
			}
			Debug.DrawRay(CharacterPosition+Vector3.up, kick);
			
			Destroy(meter.gameObject);
			Destroy(panel.gameObject);
		});
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
		panel.transform.SetParent(UIcan.transform,false);
		panel.transform.SetAsLastSibling();
		
		if ((CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount > 0)) 
		{
			if(CurrentSelectedChar.actionCount > 0|| CurrentSelectedChar.targetCount > 0) 
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
				isMoving = true;
				isPassing = false;
				if(CurrentSelectedChar.targetCount>0)
				{
					board.HighlightAdjacent (true, CurrentSelectedChar.LastTargetCell.id, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);
				}
				else board.HighlightAdjacent (true, index, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);
					
				Destroy (panel.gameObject);
			});

			if (CurrentSelectedChar.hasBall) 
			{
				Button passButton = Instantiate (buttFab) as Button;
				passButton.transform.SetParent (panel.transform, false);
				passButton.GetComponentInChildren<Text> ().text = "Pass";
				passButton.onClick.AddListener (() => 
				{ 
					isPassing = true;
					isMoving = false;
					board.HighlightAdjacent (true, index, CurrentSelectedChar.Strength);
					Destroy (panel.gameObject);
				});	
			}	
		} 
	}
	
	void OnApplicationQuit()
	{
		GameClientInstance.Disconnect();
	}
	public void NewGameButton()
	{
		this.GameClientInstance.OpJoinRandomRoom(null, 0);
	}
	public void ClearButton()
	{
		this.GameClientInstance.ClearActions();
		foreach(FSM_Character c in characters)
		{
			c.ClearActions();
		}
	}
	public void EndTurnButton()
	{
		Debug.Log("Fuck you");
		this.GameClientInstance.EndTurnEvent();
	}

	void MyCreateRoom(string roomName, byte maxPlayers)
	{
		GameClientInstance.OpCreateRoom(roomName, new RoomOptions() { MaxPlayers = maxPlayers }, TypedLobby.Default);
	}
}