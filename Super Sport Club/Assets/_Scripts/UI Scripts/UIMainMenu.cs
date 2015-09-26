using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIMainMenu : IUIState 
{
	CustomGameClient GameClientInstance;
	GUIController gui;
	RectTransform MainMenu;
	bool connectInProcess;

	public UIMainMenu(GUIController GUI, ref CustomGameClient GameClient)
	{
		this.gui = GUI;
		GameClientInstance = GameClient;
	}
	public void SetFabs (RectTransform Main, Button NewGame)
	{
		MainMenu = Main;

	}
	public void EnterState ()
	{
		MainMenu.gameObject.SetActive (true);
		if(GameClientInstance.CurrentRoom==null)
		connectInProcess = GameClientInstance.ConnectToRegionMaster("us");  // can return false for errors
	}
	public void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			Application.Quit ();
		}
	}
	public IEnumerator SlowUpdate ()
	{
		yield return null;
	}
	public void ExitState()
	{
		MainMenu.gameObject.SetActive (false);
	}
	public void ToMainMenu ()
	{
		Debug.Log ("Already in Main Menu State.");
	}
	public void ToSetPiece ()
	{
		gui.UIState = gui.UISP;
	}
	public void ToGameHUD ()
	{
		gui.UIState = gui.UIHUD;
	}
	public void NewGameButt ()
	{
		if (connectInProcess) 
		{
			this.GameClientInstance.OpJoinRandomRoom (null, 0);
			ToSetPiece ();
		}
	}
	public void DeselectCharacter()
	{
		Debug.Log ("That really doesn't apply here");
	}
	public void EndTurnButton()
	{
		Debug.Log ("There is no such button");
	}
	public void ClickOnPlayer(int id)
	{
		Debug.Log ("No interaction with player");
	}
	public void ClickOnField(Vector3 hit)
	{
		Debug.Log ("No interaction with field");
	}
}
