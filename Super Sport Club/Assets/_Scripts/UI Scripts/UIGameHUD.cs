using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIGameHUD : IUIState 
{
	static GameObject panel, meter;
	public FSM_Character CurrentSelectedChar
	{
		get{return board.GetCharacter((int)GameClientInstance.team,currentID);}
	}
	CustomGameClient GameClientInstance;
	GUIController gui;
	Canvas UICanvas;
	Button buttFab;
	Image panelFab;
	SpriteRenderer meterFab;
	Text infoText;
	float offsetX = -50,offsetY;
	bool isCharacterSelected;
	private Vector3 idealPassDir, simpleDir, offsetDir;
	private Grid_Setup board;
	private int currentID = -1;
	private bool isPassing, isMoving;

	public UIGameHUD(GUIController GUI,ref CustomGameClient GameClient)
	{
		this.gui = GUI;
		GameClientInstance = GameClient;
	}
	public void SetFabs (Canvas can, Image Panelfab, Button ButtFab, SpriteRenderer MeterFab, Text Info)
	{
		UICanvas = can;
		panelFab = Panelfab;
		buttFab = ButtFab;
		meterFab = MeterFab;
		infoText = Info;
		board = Grid_Setup.Instance;
	}
	public void EnterState ()
	{

	}

	public void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			ToMainMenu ();
		}
	}
	public void ExitState()
	{
		DeselectCharacter ();
	}
	public void ToMainMenu ()
	{
		gui.UIState = gui.UIMM;
	}
	public void ToSetPiece ()
	{
		gui.UIState = gui.UISP;
	}
	public void ToGameHUD ()
	{
		Debug.Log ("Already in Game HUD State.");
	
	}
	public void EndTurnButton()
	{
		//DeselectCharacter();
		this.GameClientInstance.EndTurnEvent();
	}
	public void ClickOnPlayer(int index)
	{
		isCharacterSelected = true;
		if(index!= currentID)
		{
			DeselectCharacter ();
			currentID = index;
			CurrentSelectedChar.Highlight(true);
		}
		CreateButtonPanel (CurrentSelectedChar.Location);
	}
	public void ClickOnField(Vector3 hit)
	{
		if(isCharacterSelected)
		{
			Cell cell = board.GetCellByLocation(hit);
			if(isPassing)
			{
				CreatePlayerMeter(cell.Location);
			}else if(isMoving)
			{
				MovementClick(cell);
			}
		}
	}
	public void DeselectCharacter()
	{
		if(currentID!=-1)
		{
			isMoving = false;
			isPassing = false;
			if (panel!=null) 
			{
				GameObject.Destroy(panel.gameObject);
			}
			if (meter!=null) 
			{
				GameObject.Destroy(meter.gameObject);
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
			GameObject.Destroy(meter.gameObject);
		}
		Vector3 loc = CharacterPosition+Vector3.up*0.2f;
		meter = GameObject.Instantiate(meterFab.gameObject, loc, Quaternion.LookRotation(simpleDir))as GameObject;
		meter.GetComponent<Gauge>().SetIdeal(idealPassDir);

		if (panel!=null) 
		{
			GameObject.Destroy(panel.gameObject);
		}

		loc= Camera.main.WorldToScreenPoint(CharacterPosition) + offsetDir*50; //re-using a variable
		panel = GameObject.Instantiate(panelFab.gameObject, loc, Quaternion.identity)as GameObject;
		panel.transform.SetParent(UICanvas.transform,false);
		panel.transform.SetAsLastSibling(); 

		Button kickButton = GameObject.Instantiate (buttFab) as Button;
		kickButton.transform.SetParent (panel.transform, false);
		kickButton.GetComponentInChildren<Text> ().text = "Kick";
		kickButton.onClick.AddListener (() => 
			{ 
				Vector3 kick = meter.GetComponent<Gauge>().StopBounce();
				Vector3 cellPos = location + kick.normalized * idealPassDir.magnitude;
				PassClick(board.GetCellByLocation(cellPos));			
				GameObject.Destroy(meter.gameObject);
				GameObject.Destroy(panel.gameObject);
			});
	}

	void CreateButtonPanel(Vector3 cellLocation)
	{
		if (panel!=null)
		{
			GameObject.Destroy(panel.gameObject);
		}
		Vector3 loc = Camera.main.WorldToScreenPoint(cellLocation);
		loc += new Vector3(offsetX,offsetY,0f);
		panel = GameObject.Instantiate(panelFab.gameObject, loc, Quaternion.identity)as GameObject;
		panel.transform.SetParent(UICanvas.transform,false);
		panel.transform.SetAsLastSibling();

		if ((CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount > 0)) 
		{
			if(CurrentSelectedChar.actionCount > 0|| CurrentSelectedChar.targetCount > 0) 
			{
				Button clearButton = GameObject.Instantiate (buttFab) as Button;
				clearButton.transform.SetParent (panel.transform, false);
				clearButton.GetComponentInChildren<Text> ().text = "Clear";
				clearButton.onClick.AddListener (() => 
					{ 
						CurrentSelectedChar.ClearActions();
						board.TurnOffHiglightedAdjacent();
						GameObject.Destroy (panel.gameObject);
					});
			}

			Button moveButton = GameObject.Instantiate (buttFab) as Button;
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
					else board.HighlightAdjacent (true, cellLocation, CurrentSelectedChar.maxActions - CurrentSelectedChar.targetCount);

					GameObject.Destroy (panel.gameObject);
				});

			if (CurrentSelectedChar.hasBall) 
			{
				Button passButton = GameObject.Instantiate (buttFab) as Button;
				passButton.transform.SetParent (panel.transform, false);
				passButton.GetComponentInChildren<Text> ().text = "Pass";
				passButton.onClick.AddListener (() => 
					{ 
						isPassing = true;
						isMoving = false;
						board.HighlightAdjacent (true, cellLocation, (int)CurrentSelectedChar.charData.Strength);
						GameObject.Destroy (panel.gameObject);
					});	
			}	
		} 
	}
}
