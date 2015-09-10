using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

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
	public Hashtable GetActionProp()
	{
		Hashtable actionProp = new Hashtable ();

		actionProp.Add("Act",(int)action);
		actionProp.Add ("iCharacter",(int)iCh.id);
		if(tCh!=null)actionProp.Add ("tCharacter",(int)tCh.id);
		if(cTo!=null)actionProp.Add("tCell",(int)cTo.id);
		if(cFrom!=null)actionProp.Add("fCell",(int)cFrom.id);
		return actionProp;
	}
}

public class FSM_Character : FSM_Base 
{
	public int id, actionCount, targetCount, maxActions = 2;
	public Cell OccupiedCell;
	[SerializeField] LayerMask layer;
	[SerializeField] protected iTween.EaseType ease;
	protected string easeType;
	GameObject[] targetPins;
	Cell lastCell;
	public Cell LastTargetCell
	{
		get{return lastCell;}
	}
	public GameObject destPin;
	[SerializeField] protected float moveSpeed;
	public bool hasTarget, hasBall;
	//Vector3 moveTarget;
	MeshRenderer currentMesh;
	Animator anim;
	public PlayerAction[] actions;
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
		currentMesh = GetComponentInChildren<MeshRenderer>();
		actions = new PlayerAction[maxActions];
		anim = GetComponentInChildren<Animator>();
		targetPins = new GameObject[maxActions];
	}
	public void SetPlayerAction(PlayerAction act)
	{
		if(actionCount<maxActions)
		{
			actions[actionCount] = act;
			actionCount += 1;
		}
	}
	public Hashtable GetCharacterAsProp()
	{
		Hashtable props = new Hashtable ();
		props["Stance"] = (int)(Stance)CurrentState;
		props["Cell"] = (int)OccupiedCell.id;

		return props;
	}

	public void SetTarget(Cell target)
	{
		targetPins[targetCount] = Instantiate(destPin,target.GetLocation(),Quaternion.identity) as GameObject;
		targetCount++;
		lastCell = target;
	}
	public void ClearActions()
	{
		for(int c= 0;c<actions.Length;c++)
		{
			actions[c] = null;
		}
		actionCount = 0;
		for(int t= 0;t<targetPins.Length;t++)
		{
			if(targetPins[t]!=null)
			Destroy(targetPins[t]);
		}
		targetCount = 0;
		lastCell = null;
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
						Vector3 target = actions[i].cTo.GetLocation();
						target += new Vector3(0,0.2f,0);
						//Move(target);
						easeType = ease.ToString();
						while (Vector3.Distance(transform.position,target)>.1f) 
						{
							if (anim!=null)
							anim.SetBool ("IsWalking",true);
							iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
							yield return new WaitForSeconds(1f);
						}
						if (anim!=null)
						anim.SetBool ("IsWalking",false);
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
		if (set) 
		{
			currentMesh.material.color = Color.cyan;
		} else 
		{
			currentMesh.material.color = Color.white;
		}
		ShowTargets(set);
	}
	void ShowTargets(bool set)
	{
		foreach (GameObject t in targetPins) 
		{
			if(t!=null)
			{
				t.GetComponent<Renderer>().enabled = set;
			}
		}
	}

//	public void Move(Vector3 target)
//	{
//		StopCoroutine("MoveTo");
//		StartCoroutine("MoveTo",target);
//	}
//	IEnumerator MoveTo(Vector3 target)
//	{
//		easeType = ease.ToString();
//		while (Vector3.Distance(transform.position,target)>.1f) 
//		{
//			iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
//			//transform.position = Vector3.MoveTowards (transform.position, target, deltaSpeed);
//			yield return null;
//		}
//	}

	public int RaycastToGround()
	{
		Ray ray = new Ray(transform.position,Vector3.down);
		RaycastHit hit = new RaycastHit();;
		Physics.Raycast(ray,out hit,5f,layer);
		return hit.transform.GetSiblingIndex();
	}
	
	void OnTriggerEnter(Collider other)
	{
		switch(other.tag)
		{
			case "Ball":
			{
				hasBall = true;
				other.transform.SetParent(transform);
				other.attachedRigidbody.isKinematic = true;
				break;
			}
		}
	}



}
