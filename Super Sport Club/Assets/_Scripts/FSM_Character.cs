using UnityEngine;
using System.Collections;

public class FSM_Character : FSM_Base 
{
	[SerializeField] protected iTween.EaseType ease;
	protected string easeType;
	public Vector3 target;
	[SerializeField] protected float moveSpeed;
	public bool hasTarget;
	Vector3 moveTarget;
	public Vector3 MoveTarget 
	{ 
		get{return moveTarget;}
		set{moveTarget = value; hasTarget = true;}
	}

	public IEnumerator MoveTo(Vector3 target)
	{
		easeType = ease.ToString();
		while (Vector3.Distance(transform.position,target)>.1f) 
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
			//transform.position = Vector3.MoveTowards (transform.position, target, deltaSpeed);
			yield return null;
		}
	}
	void Update()
	{

	}
	public void EndTurn()
	{
		if (hasTarget) 
		{
			hasTarget = false;
			StopCoroutine ("MoveTo");
			StartCoroutine ("MoveTo", moveTarget);
		}
	}

	void SetMoveTarget(Vector3 target)
	{

	}

}
