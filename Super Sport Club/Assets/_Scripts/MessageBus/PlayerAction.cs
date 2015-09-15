using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerAction
{
	public FSM_Character iCh,tCh;
	public Cell cFrom, cTo;
	public enum Actions{Move, Pass, Shoot, Juke}
	public Actions action; 
	
//	public enum ConflictFlag
//	{
//		None,
//		TargetCellIsSame,
//		TargetCellIsOriginatingCell,
//		TargetCharacterIsSame,
//		TargetCharacterIsInitiatingCharacter,
//		InitiatingCharacterIsTargetCharacter,
//		OriginatingCellIsTargetCell
//	}
//	
//	public static List<ConflictFlag> ActionsConflict(PlayerAction a, PlayerAction b)
//	{
//		List<ConflictFlag> conflicts = new List<ConflictFlag>();
//		if(a.iCh == b.tCh)
//		{
//			conflicts.Add(ConflictFlag.InitiatingCharacterIsTargetCharacter);
//		}
//		if(a.tCh == b.iCh)
//		{
//			conflicts.Add(ConflictFlag.TargetCharacterIsInitiatingCharacter);
//		}
//		if(a.tCh == b.tCh)
//		{
//			conflicts.Add(ConflictFlag.TargetCharacterIsSame);
//		}
//		if(a.cTo == b.cTo)
//		{
//			conflicts.Add(ConflictFlag.TargetCellIsSame);
//		}
//		if(a.cTo == b.cFrom)
//		{
//			conflicts.Add(ConflictFlag.TargetCellIsOriginatingCell);
//		}
//		if(a.cFrom == b.cTo)
//		{
//			conflicts.Add(ConflictFlag.OriginatingCellIsTargetCell);
//		}
//		return conflicts;
//	}
	
	public PlayerAction()
	{
	}
	public PlayerAction(Actions act, FSM_Character iCharacter)
	{
		this.action=act; iCh = iCharacter;
	}
	public PlayerAction(Actions act, FSM_Character iCharacter, FSM_Character tCharacter)
	{
		this.action=act; iCh = iCharacter; tCh = tCharacter;
	}
	public PlayerAction(Actions act, FSM_Character iCharacter, Cell tCell)
	{
		this.action=act; iCh = iCharacter; cTo = tCell;
	}
	public PlayerAction(Actions act, FSM_Character iCharacter, Cell tCell, Cell fCell)
	{
		this.action=act; iCh = iCharacter; cTo = tCell; cFrom = fCell;
	}
	public static PlayerAction PassAction(FSM_Character iCharacter, Cell tCell)
	{
		return new PlayerAction(Actions.Pass, iCharacter, tCell, iCharacter.OccupiedCell);
	}
	public Hashtable GetActionProp()
	{
		Hashtable actionProp = new Hashtable ();
		
		actionProp.Add("Act",(int)action);
		actionProp.Add ("iCharacter",(int)iCh.id);
		if(tCh!=null)actionProp.Add ("tCharacter",(int)tCh.id);
		if(cTo!=null)actionProp.Add("tCell",(int)cTo.id);
		if(cFrom!=null)actionProp.Add("fCell",(int)cFrom.id);
		return actionProp;
	}
}
