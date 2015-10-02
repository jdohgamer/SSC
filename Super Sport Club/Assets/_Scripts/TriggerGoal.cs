using UnityEngine;
using System.Collections;

public class TriggerGoal : MonoBehaviour 
{
	public Team.TeamNumber team; //This is the OPPOSING team
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag ("Ball")) 
		{
			//Scores for opponent, you suck
			UnityEventManager.TriggerEventInt ("ScoreGoal", (int)team);
		}
	}
}
