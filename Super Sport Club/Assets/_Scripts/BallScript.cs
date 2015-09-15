using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BallScript : MonoBehaviour 
{
	[SerializeField] protected iTween.EaseType ease;
	[SerializeField] protected float moveSpeed;
	protected string easeType;
	Hashtable movesHash;
	Cell targetCell;
	public Grid_Setup board;
	// Use this for initialization
	void Start () 
	{
	
	}
	public void SetMoves(Hashtable ht)
	{

	}
	// Update is called once per frame
	void Update () 
	{
		
	}

	public IEnumerator MoveTo(Hashtable ht)
	{
		targetCell = board.GetCellByID((int)ht["Cell"]);
		Vector3 target = targetCell.Location;
		moveSpeed = (float)ht["Speed"];
		easeType = (string)ht["EaseType"];
		while (Vector3.Distance(transform.position,target)>.1f) 
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
			yield return new WaitForSeconds(1f);
		}
		StopCoroutine("MoveTo");
	}
}
