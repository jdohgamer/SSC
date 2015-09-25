using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class Team 
{
	public enum TeamNumber{TeamOne=0,TeamTwo=1}
	public TeamNumber team;
	public Color teamColor;
	public FSM_Character[] mates;
	int characterCount = 0, teamSize = 5;

	public Team(TeamNumber num, Color TeamColor, int Size)
	{
		team = num;
		teamColor = TeamColor;
		mates = new FSM_Character[Size];
	}

	public void AddMate(FSM_Character newGuy)
	{
		if(this.characterCount<this.teamSize)
		{
			this.mates [characterCount] = newGuy;
			this.mates [characterCount].id = characterCount;
			this.mates [characterCount].team = team;
			this.mates [characterCount].SetColor (teamColor);
			this.characterCount++;
		}
	}
	public Hashtable GetTeamAsProps()
	{
		Hashtable TeamHT = new Hashtable ();
		for (int i = 0; i<mates.Length; i++) 
		{
			if(mates[i]!=null)
			{
				TeamHT.Add(i.ToString(),mates[i].GetCharacterAsProp());
			}
		}
		return TeamHT;
	}
}
