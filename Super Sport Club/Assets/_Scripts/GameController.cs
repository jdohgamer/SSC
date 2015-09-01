using System.Collections;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameController
{

	Grid_Setup board;
	public Grid_Setup Board{get{return board;}}
	public int length, width, currentID = -1;
	bool bWaitingOnPlayers;
	CustomGameClient gameClientInstance;
	public CustomGameClient GameClientInstance{get{return gameClientInstance;}}
	Player[] players ;
	PlayerAction[] Actions;
	FSM_Character[] characters;
	public enum Teams{TeamOne,TeamTwo}
	
	Message message;
	

	/* Singleton */
	static GameController instance = null;
	
	public static GameController Instance
	{
		get
		{
			if(instance == null)
				instance = new GameController();
			
			return instance;
		}
	}
	
	private GameController() 
	{
		players = new Player[2];
		Actions = new PlayerAction[2];
	}


	


	
}
