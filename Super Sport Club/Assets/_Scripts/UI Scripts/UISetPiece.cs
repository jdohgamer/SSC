using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UISetPiece : IUIState 
{
	GUIController gui;
	Drag[,] dragPortraits;
	bool bSetup;

	public UISetPiece(GUIController GUI)
	{
		this.gui = GUI;
		dragPortraits = new Drag[2, MainGame.Instance.TeamSize];

	}
	public void EnterState ()
	{
		if (Grid_Setup.Instance.isCreated) 
		{
			if (!bSetup)
			{
				SetupCharacterPanel ();
				ResetCharacterPanel();
			}
			else
				ResetCharacterPanel ();
		}
	}
	public void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			ToMainMenu ();
		}
		if (!bSetup && Grid_Setup.Instance.isCreated) 
		{
			SetupCharacterPanel ();
		}
	}
	public void ExitState()
	{
		Debug.Log ("Exiting SetPiece");
		gui.EnableCharacterSelection(false);
		for (int c = 0; c < MainGame.Instance.TeamSize; c++)
		{
			if (dragPortraits[MainGame.Instance.CurrentTeamNum,c] != null) 
			{
				dragPortraits [MainGame.Instance.CurrentTeamNum,c].DisableMe ();
			}
		}
	}
	public void ToMainMenu ()
	{
		gui.UIState = gui.UIMM;
	}
	public void ToShotState ()
	{
		gui.UIState = gui.UISOG;
	}
	public void ToSetPiece ()
	{
		Debug.Log ("Already in SetPiece State.");
	}
	public void ToGameHUD ()
	{
		gui.UIState = gui.UIHUD;
	}
	public void EndTurnButton()
	{
//		if (OutOfCards ()) 
//		{
//			MainGame.Instance.SubmitTeam ();
//			ToGameHUD ();
//		} else {
//			Debug.Log ("You still have players to place");
//		}

	}

	public void ClickOnPlayer(int id)
	{
		//add dragging ability for characters
	}
	public void DeselectCharacter()
	{
		//this should do something
	}
	public void ClickOnField(Vector3 hit)
	{
		//add dragging ability for characters
	}
	public void MoveCharacter()
	{
		//add dragging ability for characters
	}

//	bool OutOfCards()
//	{
//		return !(activeCards > 0);
//	}

	public bool CanPlaceCharacter(Vector3 potential)
	{
		Cell potent = Grid_Setup.Instance.GetCellByLocation (potential);
		if (potent.team == MainGame.Instance.CurrentTeamNum && !potent.bOccupied) 
		{
			return true;
		} else {
			return false;
		}
	}

	public void SetupCharacterPanel()
	{
		gui.EnableHUD (true);
		gui.EnableCharacterSelection(true);

		for(int b = 0; b<2; b++)
		{
			for (int c = 0; c < MainGame.Instance.TeamSize; c++) 
			{
				Image charObject = GameObject.Instantiate (gui.characterCardFab, Vector3.zero, Quaternion.identity) as Image;
				charObject.transform.SetParent (gui.CharacterPanel);
				dragPortraits [b,c] = charObject.GetComponent<Drag> ();
				dragPortraits [b,c].PlayerPosition = MainGame.Instance.GetCharacter (b, c).charData.Name;
				dragPortraits [b,c].index = c;
				dragPortraits [b,c].gui = this;
				dragPortraits[b,c].DisableMe();
			}
		}
		bSetup = true;
	}
	public void ResetCharacterPanel()
	{
		gui.EnableHUD (true);
		gui.EnableCharacterSelection(true);
		for (int c = 0; c < MainGame.Instance.TeamSize; c++)
		{
			if (dragPortraits[MainGame.Instance.CurrentTeamNum,c] != null) 
			{
				dragPortraits [MainGame.Instance.CurrentTeamNum,c].EnableMe();
			}
		}
		
	}
}
