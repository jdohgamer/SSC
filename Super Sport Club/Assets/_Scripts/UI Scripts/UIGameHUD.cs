using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIGameHUD : IUIState 
{
	static GameObject panel, meter;
	public UnitController CurrentSelectedChar
	{
		get{return MainGameInstance.GetCharacter((int)MainGameInstance.CurrentTeamNum,currentID);}
	}
	MainGame MainGameInstance;
	GUIController gui;
	float offsetX = -50,offsetY =0;
	bool isCharacterSelected;
	private Vector3 idealPassDir, simpleDir, offsetDir;
	private Grid_Setup board;
	private int currentID = -1;
	private bool isPassing, isMoving, isShooting, shotFired;
	//LayerMask mask = (1<<LayerMask.NameToLayer("Ground") | 1<<LayerMask.NameToLayer("Characters"));

	public UIGameHUD(GUIController GUI, MainGame mg)
	{
		this.gui = GUI;
		MainGameInstance = mg;
		board = Grid_Setup.Instance;
	}
	public void EnterState ()
	{
		UnityEventManager.StartListening("ShotFired", ShotOnGoal);
		gui.EnableHUD (true);
		gui.EnableCharacterSelection(false);
	}

	public void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			ToMainMenu ();
		}
		if (Input.GetKeyDown (KeyCode.Space)) 
		{
			ToShotState ();
		}
	}
	public void ExitState()
	{
		UnityEventManager.StopListening("ShotFired",ShotOnGoal);
		gui.EnableHUD (false);
		Debug.Log ("Leaving game State");
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
	void ShotOnGoal()
	{
		iTween.Pause ();
		if(MainGame.Instance.IsShotOnGoal(MainGameInstance.CurrentTeamNum, BallScript.TargetLocation))
		{
			ToShotState();
		}
	}
	public void ToShotState ()
	{
		gui.UIState = gui.UISOG;
	}

	public void ToGameHUD ()
	{
		Debug.Log ("Already in Game HUD State.");
	
	}
	public void EndTurnButton()
	{
		//DeselectCharacter();

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
			Cell cell = board.GetCellByLocation (hit);
			if (board.IsInsideHighlighted (cell.Location)) 
			{
				if (isPassing) 
				{
					CreatePlayerMeter (cell.Location, PlayerAction.Actions.Pass);
				} else if (isMoving) 
				{
					MovementClick (cell);
				} else if (isShooting) 
				{
					CreatePlayerMeter (cell.Location, PlayerAction.Actions.Shoot);
				}
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
		MainGameInstance.SetPlayerAction(new PlayerAction(PlayerAction.Actions.Move, CurrentSelectedChar, tCell, CurrentSelectedChar.OccupiedCell));
		if ((CurrentSelectedChar.targetCount == 1 && CurrentSelectedChar.IsSprinting))  
		{
			board.HighlightAdjacent (true, tCell.Location, CurrentSelectedChar.MoveDistance);

		}
		else {
			board.TurnOffHiglightedAdjacent();
			isMoving = false;
		}
	}	

	void KickClick(Cell tCell, PlayerAction.Actions act)
	{
		if (CurrentSelectedChar.maxActions -CurrentSelectedChar.actionCount > 0) 
		{
			MainGameInstance.SetPlayerAction(new PlayerAction (act, CurrentSelectedChar,tCell));
			board.TurnOffHiglightedAdjacent();
			isPassing = false;
		}
	}

	void CreatePlayerMeter(Vector3 location, PlayerAction.Actions act)
	{
		Debug.Log(location);
		Vector3 CharacterPosition = CurrentSelectedChar.LastTargetCell.Location;
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
		meter = GameObject.Instantiate(gui.meterFab.gameObject, loc, Quaternion.LookRotation(simpleDir))as GameObject;
		meter.GetComponent<Gauge>().SetIdeal(idealPassDir);

		if (panel!=null) 
		{
			GameObject.Destroy(panel.gameObject);
		}

		loc= Camera.main.WorldToScreenPoint(CharacterPosition) + offsetDir*50; //re-using a variable
		panel = GameObject.Instantiate(gui.panelFab.gameObject, loc, Quaternion.identity)as GameObject;
		panel.transform.SetParent(gui.UIcan.transform,false);
		panel.transform.SetAsLastSibling(); 
		PanelController pc = panel.GetComponent<PanelController> ();

		pc.AddButton("Kick", false).onClick.AddListener (() => 
		{ 
			Vector3 kick = meter.GetComponent<Gauge>().StopBounce();
			Vector3 cellPos = CharacterPosition + kick.normalized * idealPassDir.magnitude;
			KickClick(board.GetCellByLocation(cellPos),act);			
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
		panel = GameObject.Instantiate(gui.panelFab.gameObject, loc, Quaternion.identity)as GameObject;//prefab with PanelController script attached
		panel.transform.SetParent(gui.UIcan.transform,false);
		panel.transform.SetAsLastSibling();
		PanelController pc = panel.GetComponent<PanelController> ();
		Vector3 CharacterPosition = CurrentSelectedChar.LastTargetCell.Location;

		if(CurrentSelectedChar.actionCount > 0|| CurrentSelectedChar.targetCount > 0) 
		{
			pc.AddButton("Clear", false).onClick.AddListener (() => 
			{ 
				CurrentSelectedChar.ClearActions();
				board.TurnOffHiglightedAdjacent();
				GameObject.Destroy (panel.gameObject);
			});
		}
		if (MainGameInstance.ActionsLeft > 0) 
		{
			if ((CurrentSelectedChar.maxActions - CurrentSelectedChar.actionCount > 0))
			{
				if(CurrentSelectedChar.targetCount <2 && CurrentSelectedChar.CanSprint  && !CurrentSelectedChar.IsSprinting)
				{
					pc.AddButton ("Sprint", false).onClick.AddListener (() => 
						{ 
							CurrentSelectedChar.StartSprinting();
							GameObject.Destroy (panel.gameObject);
						});
				}
				if (CurrentSelectedChar.targetCount == 0 || (CurrentSelectedChar.targetCount == 1 && CurrentSelectedChar.IsSprinting)) 
				{
					pc.AddButton ("Move", false).onClick.AddListener (() => 
					{ 
						isMoving = true;
						isShooting = false;
						isPassing = false;
						board.HighlightAdjacent (true, CharacterPosition, CurrentSelectedChar.MoveDistance);
						GameObject.Destroy (panel.gameObject);
					});
				}

				if (CurrentSelectedChar.hasBall || CurrentSelectedChar.LastTargetCell.IsVectorInCell(board.BallLocation)) 
				{
					pc.AddButton("Pass", false).onClick.AddListener (() => 
					{ 
						isPassing = true;
						isShooting = false;
						isMoving = false;
						board.HighlightAdjacent (true,  CharacterPosition, (int)CurrentSelectedChar.charData.Strength);
						GameObject.Destroy (panel.gameObject);
					});	
					pc.AddButton("Shoot", false).onClick.AddListener (() => 
					{ 
						isShooting = true;
						isPassing = false;
						isMoving = false;
						board.HighlightAdjacent (true,  CharacterPosition, (int)CurrentSelectedChar.charData.Strength);
						GameObject.Destroy (panel.gameObject);
					});	
				}
			}
		} 
	}
}
