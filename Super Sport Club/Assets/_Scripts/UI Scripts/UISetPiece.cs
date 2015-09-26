using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UISetPiece : IUIState 
{
	RectTransform CharacterPanel;
	Image characterCardFab;
	CustomGameClient GameClientInstance;
	GUIController gui;
	Text infoText;
	Drag[] dragPortraits;
	int activeCards = 0;
	bool bSetup;

	public UISetPiece(GUIController GUI,ref CustomGameClient GameClient)
	{
		this.gui = GUI;
		GameClientInstance = GameClient;
		dragPortraits = new Drag[Grid_Setup.Instance.TeamSize];

	}
	public void EnterState ()
	{
		if (Grid_Setup.Instance.isCreated) 
		{
			if (!bSetup)
				SetupCharacterPanel ();
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
		CharacterPanel.gameObject.SetActive (false);
		for (int c = 0; c < dragPortraits.Length; c++) 
		{
			dragPortraits [c].DisableMe();
		}
	}
	public void ToMainMenu ()
	{
		gui.UIState = gui.UIMM;
	}
	public void ToSetPiece ()
	{
		Debug.Log ("Already in SetPiece State.");
	}
	public void ToGameHUD ()
	{
		gui.UIState = gui.UIHUD;
	}
	public void SetFabs (RectTransform charPan, Image cardFab, Text Info)
	{
		CharacterPanel = charPan;
		characterCardFab = cardFab;
		infoText = Info;
	}
	public void EndTurnButton()
	{
		if (OutOfCards ()) 
		{
			this.GameClientInstance.SubmitTeamEvent ();
			ToGameHUD ();
		} else {
			Debug.Log ("You still have players to place");
		}

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
	}
	public void MoveCharacter()
	{
		//add dragging ability for characters
	}

	bool OutOfCards()
	{
		return !(activeCards > 0);
	}
	public bool PlaceCharacter(Vector3 potential)
	{
		if (Grid_Setup.Instance.GetCellByLocation (potential).team == GameClientInstance.team) 
		{
			activeCards--;
			return true;
		} else {
			return false;
		}
	}

	public void SetupCharacterPanel()
	{
		CharacterPanel.gameObject.SetActive (true);

		for (int c = 0; c < dragPortraits.Length; c++) 
		{
			Image charObject = GameObject.Instantiate (characterCardFab, Vector3.zero, Quaternion.identity) as Image;
			charObject.transform.SetParent (CharacterPanel);
			dragPortraits [c] = charObject.GetComponent<Drag> ();
			dragPortraits [c].PlayerPosition = Grid_Setup.Instance.GetCharacter ((int)GameClientInstance.team, c).charData.Name;
			dragPortraits [c].index = c;
			dragPortraits [c].gameClient = GameClientInstance;
			dragPortraits [c].gui = this;
			activeCards++;
		}
		bSetup = true;
	}
	public void ResetCharacterPanel()
	{
		CharacterPanel.gameObject.SetActive (true);
		for (int c = 0; c < dragPortraits.Length; c++) 
		{
			dragPortraits [c].EnableMe();
			activeCards++;
		}
	}
}
