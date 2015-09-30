using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class Team 
{
	public enum TeamNumber{TeamOne=0,TeamTwo=1, NONE = -1}
	public TeamNumber teamNum;
	public Color teamColor;
	public FSM_Character[] mates;
	public Bounds GoalArea;
	int characterCount = 0, teamSize = 5;

	public Team(TeamNumber num, Color TeamColor, int Size, Vector3 goalCenter, Vector3 goalSize)
	{
		teamNum = num;
		teamColor = TeamColor;
		mates = new FSM_Character[Size];
		GoalArea = new Bounds (goalCenter, goalSize);
	}

	public void AddMate(FSM_Character newGuy)
	{
		if(this.characterCount<this.teamSize)
		{
			this.mates [characterCount] = newGuy;
			this.mates [characterCount].id = characterCount;
			this.mates [characterCount].team = teamNum;
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
	public bool IsVectorInGoal(Vector3 spot)
	{
		return GoalArea.Contains(spot);
	}
}
