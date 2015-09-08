using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class FSM_Character : FSM_Base 
{
	public int id, actionCount, targetCount, maxActions = 2;
	public int Strength = 5, Speed = 6, Defense = 4;
	public Cell OccupiedCell;
	public bool hasTarget, hasBall;
	[HideInInspector]public BallScript ball;
	public Cell LastTargetCell
	{
		get{return lastCell;}
	}
	public enum Stance
	{
		Neutral,
		Move,
		Sprint,
		Defend,
		Pass,
		Shoot,
		Cover_Man,
		Cover_Ball
	};
	protected string easeType;
	[SerializeField] protected float moveSpeed;
	[SerializeField] protected iTween.EaseType ease, ballEase;
	[SerializeField] LayerMask groundLayer, characterLayer;
	[SerializeField] GameObject destPin;
	GameObject[] targetPins;
	static GameObject passTargetPin;
	PlayerAction[] actions;
	Cell lastCell;
	MeshRenderer currentMesh;
	Animator anim;
	Transform tran;
	FSM_Character opp;

	void Awake()
	{
		CurrentState = Stance.Neutral;
		tran = transform;
		currentMesh = GetComponentInChildren<MeshRenderer>();
		anim = GetComponentInChildren<Animator>();
		actions = new PlayerAction[maxActions];
		targetPins = new GameObject[maxActions];
		passTargetPin = Instantiate(destPin,Vector3.zero,Quaternion.identity) as GameObject;
		passTargetPin.SetActive(false);
	}
	void Update()
	{
		Debug.DrawRay(tran.position,tran.forward);
	}

	public void SetPlayerAction(PlayerAction act)
	{
		if(actionCount<maxActions)
		{
			actions[actionCount] = act;
			actionCount += 1;
		}
	}

	public void SetMoveTarget(Cell target)
	{
		targetPins[targetCount] = Instantiate(destPin,target.GetLocation(),Quaternion.identity) as GameObject;
		targetCount++;
		lastCell = target;
	}
	public void SetPassTarget(Cell target)
	{
		passTargetPin.SetActive(true);
		passTargetPin.transform.position = target.GetLocation();
	}

	public Hashtable GetCharacterAsProp()
	{
		Hashtable props = new Hashtable ();
		props["Stance"] = (int)(Stance)CurrentState;
		props["Cell"] = (int)OccupiedCell.id;

		return props;
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
		passTargetPin.SetActive(false);
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
						RotateTowards(target);
						
						easeType = ease.ToString();
						while (Vector3.Distance(tran.position,target)>.1f) 
						{
							if(CanMove(actions[i].cTo))
							{
								iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
								yield return null;
							}else break;
						}
						break;
					}
					case PlayerAction.Actions.Pass:
					{
						if(hasBall)
						{
							Hashtable ht = new Hashtable();
							ht["Speed"] = (float)Strength;
							ht["Cell"] = actions[i].cTo.id;
							ht["EaseType"] = ballEase.ToString();
							ball.StartCoroutine("MoveTo",ht);
						}
						break;
					}
				}
			}else break;
		}
		ClearActions();	
		yield return null;
	}
	void RotateTowards(Vector3 target)
	{
		Vector3 dir = target - tran.position;
		Quaternion rotation = Quaternion.LookRotation(dir);
		tran.rotation= rotation;
		//float f = Vector3.Angle(tran.forward,dir);
		//tran.Rotate(Vector3.up,f);
	}

	bool CanMove(Cell targetCell)
	{
		bool canMove;
		opp = PlayerInFrontOfMe();
		if(opp!=null)
		{
			Debug.Log("Player ID: "+opp.id);
			if(targetCell.GetLocation() == opp.transform.position)
			{
				if(targetCell==opp.LastTargetCell)
				{
					canMove = false;
				}else{
					canMove = true;
				}
			}else{
				float dotFace = Vector3.Dot(tran.forward.normalized,opp.transform.forward.normalized);
				if(dotFace<0)
				{
					Debug.Log("Oh, just kiss already");
					canMove = false;
				}else{
					canMove = true;
				}
			}
		}else canMove = true;
		return canMove;
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
//			yield return new WaitForSeconds(1f);
//		}
//		StopCoroutine("MoveTo");
//	}

	public int RaycastToGround()
	{
		Ray ray = new Ray(tran.position,Vector3.down);
		RaycastHit hit = new RaycastHit();;
		Physics.Raycast(ray,out hit,5f,groundLayer);
		return hit.transform.GetSiblingIndex();
	}

	FSM_Character PlayerInFrontOfMe()
	{
		Ray ray = new Ray(tran.position,tran.forward);
		RaycastHit hit = new RaycastHit();;
		Physics.Raycast(ray,out hit,1.5f,characterLayer);
		if(hit.collider!=null&& hit.collider!= this.GetComponent<Collider>() && hit.transform.tag == "Player")
		{
			return hit.transform.GetComponent<FSM_Character>();
		}
		return null;
	}

//	IEnumerator Move_EnterState()
//	{
//
//	}
	void OnTriggerEnter(Collider other)
	{
		switch(other.tag)
		{
			case "Ball":
			{
				hasBall = true;
				other.transform.SetParent(transform);
				other.transform.position = transform.TransformPoint(0,0,1);
				ball = other.GetComponent<BallScript>();
				break;
			}
		}
	}
	void OnTriggerExit(Collider other)
	{
		switch(other.tag)
		{
			case "Ball":
			{
				hasBall = false;
				other.transform.SetParent(null);
				ball = null;
				break;
			}
		}
	}



}
