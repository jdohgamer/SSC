using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIMainMenu : IUIState 
{
	GUIController gui;


	public UIMainMenu(GUIController GUI)
	{
		this.gui = GUI;
	}
	public void EnterState ()
	{
		gui.EnableMainMenu (true);
		//if(GameClientInstance.CurrentRoom==null)
		//connectInProcess = CustomGameClient.ClientInstance.ConnectToRegionMaster("us");  // can return false for errors
	}
	public void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) 
		{
			Application.Quit ();
		}
	}
	public void ExitState()
	{
		gui.EnableMainMenu(false);
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
	public void ToShotState ()
	{
		gui.UIState = gui.UISOG;
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
