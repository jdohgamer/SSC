using UnityEngine;
using System.Collections;

public class UIShotState : IUIState 
{
	GUIController gui;
	Animator camAnim;

	public UIShotState(GUIController GUI)
	{
		this.gui = GUI;
		camAnim = Camera.main.GetComponent<Animator> ();
	}
	public void EnterState ()
	{
		//animate to goal
		int pan = MainGame.Instance.CurrentTeamNum>0 ? 1:-1 ;
		camAnim.SetInteger("SwirlDir", pan);

		gui.EnableHUD (true);
	}
	public void Update ()
	{
		//wait for player response
	}
	public void ExitState ()
	{
		//move camera back
		camAnim.SetInteger("SwirlDir", 0);
		//maybe don't move if going to menu screen?
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
		iTween.Resume ();
		ToGameHUD();
	}
	public void ClickOnPlayer(int id)
	{
		Debug.Log ("No Character Selection");
	}
	public void DeselectCharacter()
	{
		Debug.Log ("No Character Selection");
	}
	public void ClickOnField(Vector3 hit)
	{
		Debug.Log ("No Field Selection");
	}

}
