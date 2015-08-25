using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GUIController: MonoBehaviour
{
	private CustomGameClient GameClientInstance;
	public string AppId;            // set in inspector
	// this is called when the client loaded and is ready to start
	[SerializeField] LayerMask mask;
	[SerializeField] Image panelFab;
	[SerializeField] Button buttFab;
	[SerializeField] Canvas can;
	int currentID = -1;
	FSM_Character[] characters;
	public FSM_Character CurrentSelectedChar
	{
		get{return characters[currentID];}
	}
	
	public float offsetX,offsetY;
	public static GameObject panel;
	public float serviceInterval = 1;
	public float timeSinceService;

	Grid_Setup board;
	public static bool bSelection;

	void Awake()
	{
		this.board = GetComponent<Grid_Setup>();
		this.GameClientInstance  = new CustomGameClient();
		this.GameClientInstance.AppId = AppId;  // edit this!
		this.GameClientInstance.board = board;
		this.GameClientInstance.gui = this;

		Application.runInBackground = true;
		CustomTypes.Register();
		// "eu" is the European region's token
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
	void Start()
	{
	
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
			//message.Clear();
			
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
		}
		if (Input.GetMouseButtonUp (1)) 
		{
			//actionCount = 0; //make a button
		}
	}


void MovementClick(int index)
{
	if (CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount > 0) 
	{
		GameClientInstance.SetPlayerAction(PlayerAction.Actions.Move, CurrentSelectedChar, board.cells[index]);
		board.HighlightAdjacent (true, index, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount );
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
			if(CurrentSelectedChar.targetCount>0)
			{
				board.HighlightAdjacent (true, CurrentSelectedChar.LastTargetCell.id, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);
			}
			else board.HighlightAdjacent (true, index, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);
				
			Destroy (panel.gameObject);
		});

		
		if (CurrentSelectedChar.hasBall) {
			Button passButton = Instantiate (buttFab) as Button;
			passButton.transform.SetParent (panel.transform, false);
			passButton.GetComponentInChildren<Text> ().text = "Pass";
			passButton.onClick.AddListener (() => 
			                                { 
			  GameClientInstance.SetPlayerAction (PlayerAction.Actions.Pass, CurrentSelectedChar, board.cells [index]);
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
		//this.GameClientInstance.CreateTurnbasedRoom();
		this.GameClientInstance.OpJoinRandomRoom(null, 0);
	}

	public void ClearButton()
	{
		//this.GameClientInstance.CreateTurnbasedRoom();
		this.GameClientInstance.ClearActions();
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