using UnityEngine;
using System.Collections;

public interface IUIState
{
	void EnterState ();
	void Update ();
	void ExitState();
	void ToMainMenu ();
	void ToSetPiece ();
	void ToGameHUD ();
	void EndTurnButton();
	void ClickOnPlayer(int id);
	void DeselectCharacter();
	void ClickOnField(Vector3 hit);
}
