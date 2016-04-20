using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[System.Serializable]
public class Team 
{
	public UnitController[] mates;
	Color teamColor;
	Bounds goalArea;
	int characterCount = 0, teamSize = 5;
	Quaternion face;

	public Team(int num, Color TeamColor, int Size, Vector3 goalCenter, Vector3 goalSize)
	{
		teamColor = TeamColor;
		mates = new UnitController[Size];
		goalArea = new Bounds (goalCenter, goalSize);
		face = num==0 ? Quaternion.LookRotation(Vector3.right):Quaternion.LookRotation(-Vector3.right) ;
	}

	public void AddMate(UnitController newGuy)
	{
		if(this.characterCount<this.teamSize)
		{
			this.mates [characterCount] = newGuy;
			this.mates [characterCount].transform.rotation = face;
			this.mates [characterCount].id = characterCount;
			this.mates [characterCount].SetColor (teamColor);
			this.characterCount++;
		}
	}
	public void Sleep()
	{
		for (int t = 0; t < teamSize; t++) 
		{
			mates [t].transform.rotation = face;
			mates [t].gameObject.SetActive (false);
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
		return goalArea.Contains(spot);
	}
}
