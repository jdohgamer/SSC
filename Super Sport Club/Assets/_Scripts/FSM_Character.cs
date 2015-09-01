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

	public enuSm Stance
	{
		Neutral,
		Sprint,
		Pass,
		Shoot,
		Cover_Man,
		Cover_Ball
	};

	public void Awake()
	{
		CurrentState = Stance.Neutral;
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

	public void EndTurn()
	{
		if (hasTarget) 
		{
			hasTarget = false;
			StopCoroutine ("MoveTo");
			StartCoroutine ("MoveTo", moveTarget);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		switch(other.tag)
		{
			case "Ball":
			{
				other.transform.SetParent(transform);
				other.attachedRigidbody.isKinematic = true;
				break;
			}
		}
	}

	void SetMoveTarget(Vector3 target)
	{

	}

}
