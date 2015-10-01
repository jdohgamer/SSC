using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BallScript : MonoBehaviour 
{
	//[SerializeField] protected iTween.EaseType ease;
	public static Vector3 TargetLocation;
	[SerializeField] protected float moveSpeed;
	[SerializeField] float offset = 0.1f;
	protected string easeType;
	Hashtable movesHash;
	Cell targetCell;
	bool canMove = false;


	public void StopMe()
	{
		canMove = false;
		iTween.Stop (this.gameObject);
		StopCoroutine("MoveTo");
	}
	public IEnumerator MoveTo(Hashtable ht)
	{
		canMove = true;
		targetCell = Grid_Setup.Instance.GetCellByID((int)ht["Cell"]);
		moveSpeed = (float)ht["Speed"];
		easeType = (string)ht["EaseType"];
		TargetLocation = targetCell.Location;
		TargetLocation.y += offset;
		while (Vector3.Distance(transform.position,TargetLocation)>.1f && canMove) 
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", TargetLocation, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
			yield return new WaitForSeconds(1f);
		}
		StopCoroutine("MoveTo");
	}
}
