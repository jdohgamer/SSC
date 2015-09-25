using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GUIController: MonoBehaviour
{
	public static bool isCharacterSelected;
	public static GameObject panel, meter;
	public float timeSinceService;
	public FSM_Character CurrentSelectedChar
	{
		get{return board.GetCharacter((int)GameClientInstance.team,currentID);}
	}
	[SerializeField] string AppId;// set in inspector. this is called when the client loaded and is ready to start
	[SerializeField] float offsetX,offsetY;
	[SerializeField] float serviceInterval = 1;
	[SerializeField] LayerMask mask;
	[SerializeField] Image panelFab, characterCardFab;
	[SerializeField] SpriteRenderer meterFab;
	[SerializeField] Button buttFab;
	[SerializeField] Canvas UIcan;
	[SerializeField] RectTransform MainMenu, CharacterPanel;
	//Drag[] cards;
	private Vector3 idealPassDir, simpleDir, offsetDir;
	private CustomGameClient GameClientInstance;
	private Grid_Setup board;
	private int currentID = -1;
	private bool isPassing, isMoving, isSetupPhase;


	void Awake()
	{
		this.board = GetComponent<Grid_Setup>();
		Grid_Setup.Instance = this.board;
		this.GameClientInstance  = new CustomGameClient();
		this.GameClientInstance.AppId = AppId;  // edit this!
		this.GameClientInstance.board = board;
		this.GameClientInstance.gui = this;

		Application.runInBackground = true;
		CustomTypes.Register();
		bool connectInProcess = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors
//		GameObject[] charObjs = GameObject.FindGameObjectsWithTag("CharacterCard");
//		cards = new Drag[charObjs.Length];
//		for(int i = 0; i< charObjs.Length;i++)
//		{
//			Drag temp = charObjs[i].GetComponent<Drag>();
//			if(temp!= null)
//			{
//				cards[i] = temp;
//				cards[i].gameClient = GameClientInstance;
//				cards[i].PlayerPosition = Positions[i];
//				cards[i].index = i;
//			}
//		}
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

	void FixedUpdate() 
	{
		if(GameClientInstance.CurrentRoom!=null)
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
							isCharacterSelected = true;
							FSM_Character character = hit.transform.gameObject.GetComponent<FSM_Character>();
							int id = character.id;
							if(id!= currentID)
							{
								DeselectCharacter ();
								currentID = id;
								CurrentSelectedChar.Highlight(true);
							}
							//if CurrentSelectedChar.team == GameClientInstance.team
							CreateButtonPanel(CurrentSelectedChar.OccupiedCell);
							//else CreatePlayerInfoPanel(CurrentSelectedChar.OccupiedCell) //show some info about character to opposing player
							break;
							
						}
						case "Field":
						{
							if(!EventSystem.current.IsPointerOverGameObject())
							{
								if(isCharacterSelected)
								{
									Cell cell = board.GetCellByLocation(hit.point);
									if(isPassing)
									{
										CreatePlayerMeter(cell.Location);
									}else if(isMoving)
									{
										MovementClick(cell);
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
	}

	public void SetupCharacterPanel()
	{
		for (int c = 0; c < Grid_Setup.Instance.Teams [(int)GameClientInstance.team].mates.Length; c++) 
		{
			CharacterPanel.gameObject.SetActive (true);
			Image charObject = Instantiate (characterCardFab, Vector3.zero, Quaternion.identity) as Image;
			charObject.transform.SetParent (CharacterPanel);
			Drag drag = charObject.GetComponent<Drag> ();
			drag.PlayerPosition = Grid_Setup.Instance.GetCharacter((int)GameClientInstance.team,c).charData.Name;
			drag.index = c;
			drag.gameClient = GameClientInstance;
		}
	}

	void DeselectCharacter()
	{
		if(currentID!=-1)
		{
			isMoving = false;
			isPassing = false;
			if (panel!=null) 
			{
				Destroy(panel.gameObject);
			}
			if (meter!=null) 
			{
				Destroy(meter.gameObject);
			}
			CurrentSelectedChar.Highlight(false);
			board.TurnOffHiglightedAdjacent();
			currentID = -1;
		}
	}
	
	void MovementClick(Cell tCell)
	{
		int actsLeft = CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount;
		if (actsLeft > 0) 
		{
			GameClientInstance.SetPlayerAction(new PlayerAction(PlayerAction.Actions.Move, CurrentSelectedChar, tCell));
			actsLeft--;
			if (actsLeft <= 0) 
			{
				board.TurnOffHiglightedAdjacent();
				isMoving = false;
			}else{
				board.HighlightAdjacent (true, tCell.Location, actsLeft);
			}
		} 
	}	

	void PassClick(Cell tCell)
	{
		if (CurrentSelectedChar.maxActions -CurrentSelectedChar.actionCount > 0) 
		{
			GameClientInstance.SetPlayerAction(new PlayerAction(PlayerAction.Actions.Pass, CurrentSelectedChar, tCell, CurrentSelectedChar.OccupiedCell));
			board.TurnOffHiglightedAdjacent();
			isPassing = false;
		}
	}

	void CreatePlayerMeter(Vector3 location)
	{
		Vector3 CharacterPosition = CurrentSelectedChar.Location;
		idealPassDir = location - CharacterPosition;
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
		Vector3 loc = CharacterPosition+Vector3.up*0.2f;
		meter = Instantiate(meterFab.gameObject, loc, Quaternion.LookRotation(simpleDir))as GameObject;
		meter.GetComponent<Gauge>().SetIdeal(idealPassDir);
		
		if (panel!=null) 
		{
			Destroy(panel.gameObject);
		}
		
		loc= Camera.main.WorldToScreenPoint(CharacterPosition) + offsetDir*50; //re-using a variable
		panel = Instantiate(panelFab.gameObject, loc, Quaternion.identity)as GameObject;
		panel.transform.SetParent(UIcan.transform,false);
		panel.transform.SetAsLastSibling(); 
		
		Button kickButton = Instantiate (buttFab) as Button;
		kickButton.transform.SetParent (panel.transform, false);
		kickButton.GetComponentInChildren<Text> ().text = "Kick";
		kickButton.onClick.AddListener (() => 
		{ 
			Vector3 kick = meter.GetComponent<Gauge>().StopBounce();
			Vector3 cellPos = location + kick.normalized * idealPassDir.magnitude;
			PassClick(board.GetCellByLocation(cellPos));			
			Destroy(meter.gameObject);
			Destroy(panel.gameObject);
		});
	}

	void CreateButtonPanel(Cell cell)
	{
		if (panel!=null)
		{
			Destroy(panel.gameObject);
		}
		Vector3 loc = Camera.main.WorldToScreenPoint(cell.Location);
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
					board.TurnOffHiglightedAdjacent();
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
					board.HighlightAdjacent (true, CurrentSelectedChar.LastTargetCell.Location, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);
				}
				else board.HighlightAdjacent (true, cell.Location, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);
					
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
					board.HighlightAdjacent (true, cell.Location, (int)CurrentSelectedChar.charData.Strength);
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
		MainMenu.gameObject.SetActive(false);
		isSetupPhase = true;
		this.GameClientInstance.OpJoinRandomRoom(null, 0);
	}
	public void QuitGameButton()
	{
		Application.Quit();
	}
	public void ClearButton()
	{
		this.GameClientInstance.ClearActions();
		foreach(FSM_Character c in board.Teams[(int)GameClientInstance.team].mates)
		{
			c.ClearActions();
		}
	}
	public void EndTurnButton()
	{
		if(isSetupPhase)
		{
			isSetupPhase = false;
			this.GameClientInstance.SubmitTeamEvent();
		}else
		{
			DeselectCharacter();
			this.GameClientInstance.EndTurnEvent();
		}
	}

	void MyCreateRoom(string roomName, byte maxPlayers)
	{
		GameClientInstance.OpCreateRoom(roomName, new RoomOptions() { MaxPlayers = maxPlayers }, TypedLobby.Default);
	}
}