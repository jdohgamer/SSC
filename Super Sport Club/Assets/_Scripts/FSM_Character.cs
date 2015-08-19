using UnityEngine;
using System.Collections;

public class PlayerAction
{
	public FSM_Character iCh,tCh;
	public Cell cFrom, cTo;
	public enum Actions{Move, Pass, Shoot, Juke}
	public Actions action; 
	
	public PlayerAction()
	{
		
	}
	public PlayerAction(Actions act, FSM_Character iCharacter)
	{
		this.action=act; iCh = iCharacter;
	}
	public PlayerAction(Actions act, FSM_Character iCharacter, FSM_Character tCharacter)
	{
		this.action=act; iCh = iCharacter; tCh = tCharacter;
	}
	public PlayerAction(Actions act, FSM_Character iCharacter, Cell tCell)
	{
		this.action=act; iCh = iCharacter; cTo = tCell;
	}
	public static PlayerAction MoveAction(FSM_Character iCharacter, Cell tCell)
	{
		return new PlayerAction(Actions.Move, iCharacter, tCell);
	}
}

public class FSM_Character : FSM_Base 
{
	public int id;
	public Cell OccupiedCell;
	[SerializeField] protected iTween.EaseType ease;
	protected string easeType;
	public Vector3 target;
	[SerializeField] protected float moveSpeed;
	public bool hasTarget;
	Vector3 moveTarget;
	MeshRenderer currentMesh;
	public PlayerAction[] actions;
	int actionCount;
	public Vector3 MoveTarget 
	{ 
		get{return moveTarget;}
		set{moveTarget = value; hasTarget = true;}
	}
	GameController.Teams team;

	public enum Stance
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
		currentMesh = GetComponent<MeshRenderer>();
		actions = new PlayerAction[2];
	}
	public void SetPlayerAction(PlayerAction act)
	{
		if(actionCount<2)
		{
			actions[actionCount] = act;
			actionCount += 1;
		}
	}
	void ClearActions()
	{
		for(int c= 0;c<actions.Length;c++)
		{
			actions[c] = null;
		}
		actionCount = 0;
	}
	public IEnumerator ExecuteActions()
	{
		for (int i = 0;i<actions.Length;i++)
		{
			if(actions[i]!=null)
			{
				switch(actions[i].action)
				{
					case PlayerAction.Actions.Move:
					{
						target = actions[i].cTo.GetLocation();
						//Move(target);
						easeType = ease.ToString();
						while (Vector3.Distance(transform.position,target)>.1f) 
						{
							iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
							yield return new WaitForSeconds(1f);
						}
						break;
					}
				}
			}else break;
		}
		ClearActions();	
		yield return null;


	}

	public void Highlight(bool set)
	{
		if(set)
			currentMesh.material.color = Color.cyan;
		else
			currentMesh.material.color = Color.white;
	}

	public void Move(Vector3 target)
	{
		StopCoroutine("MoveTo");
		StartCoroutine("MoveTo",target);
	}
	IEnumerator MoveTo(Vector3 target)
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



}
