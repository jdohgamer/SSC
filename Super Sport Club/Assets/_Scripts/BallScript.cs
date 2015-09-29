using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BallScript : MonoBehaviour 
{
	//[SerializeField] protected iTween.EaseType ease;
	[SerializeField] protected float moveSpeed;
	[SerializeField] float offset = 0.2f;
	protected string easeType;
	Hashtable movesHash;
	Cell targetCell;

	public void SetMoves(Hashtable ht)
	{

	}

	public IEnumerator MoveTo(Hashtable ht)
	{
		targetCell = Grid_Setup.Instance.GetCellByID((int)ht["Cell"]);
		moveSpeed = (float)ht["Speed"];
		easeType = (string)ht["EaseType"];
		Vector3 target = targetCell.Location;
		target.y += offset;
		while (Vector3.Distance(transform.position,target)>.1f) 
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
			yield return new WaitForSeconds(1f);
		}
		StopCoroutine("MoveTo");
	}
}
