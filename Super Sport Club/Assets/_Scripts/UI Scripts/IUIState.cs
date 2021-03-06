﻿using UnityEngine;
using System.Collections;

public interface IUIState
{
	void EnterState ();
	void Update ();
	//void UpdateHUD ();
	void ExitState ();
	void ToMainMenu ();
	void ToSetPiece ();
	void ToGameHUD ();
	void ToShotState ();
	void EndTurnButton();
	void ClickOnPlayer(int id);
	void DeselectCharacter();
	void ClickOnField(Vector3 hit);
}
