using UnityEngine;
using System.Collections;

public class UIShotState : IUIState 
{
	CustomGameClient GameClientInstance;
	GUIController gui;
	Animator camAnim;

	public UIShotState(GUIController GUI, CustomGameClient GameClient)
	{
		this.gui = GUI;
		GameClientInstance = GameClient;
		camAnim = Camera.main.GetComponent<Animator> ();
	}
	public void EnterState ()
	{
		//animate to goal
		int pan = (int)GameClientInstance.team>0 ? 1:-1 ;
		camAnim.SetInteger("PanDir", pan);

		gui.EnableHUD (true);
	}
	public void Update ()
	{
		//wait for player response
	}
	public void ExitState ()
	{
		//move camera back
		camAnim.SetFloat("PanDir", 0);
		//maybe don't move if going menu screen?
	}
	public void ToMainMenu ()
	{
		gui.UIState = gui.UIMM;
	}
	public void ToSetPiece ()
	{
		//This probably won't happen.
		gui.UIState = gui.UISP;
	}
	public void ToGameHUD ()
	{
		//the default state to return to
		gui.UIState = gui.UIHUD;
	}
	public void ToShotState ()
	{
		Debug.Log ("Already in Shot on Goal");
	}
	public void EndTurnButton()
	{
		//send result
		ToGameHUD();
	}
	public void ClickOnPlayer(int id)
	{
		Debug.Log ("Nope");
	}
	public void DeselectCharacter()
	{
		Debug.Log ("Nope");
	}
	public void ClickOnField(Vector3 hit)
	{
		Debug.Log ("Nope");
	}

}
