using UnityEngine;
using System.Collections;

public class NPC_FSM : FSM_Base
{
	/*
	AIPath AStar;
	Anim anim;
	SphereCollider col;
	//GameObject[] aVisibleFood = new GameObject[10];
	Queue qVisibleFood = new Queue(20);
	ArrayList aVisibleFood = new ArrayList(20);
	GameObject[] waypoints;
	public int count;
	public Transform target;
	public float pushPower = 2.0F;
	public float hunger;
	bool bHungry;
	bool bFoodVisible;
	float fieldOfViewAngle = 110;
	Vector3 dLocation;
	public string myState;
	public bool bBusy = false;
	public bool bDanger = false;
	public enum State{Eating,Hungry,Idle,Moving};

	void Awake()
	{
		AStar = GetComponent<AIPath>();
		col = GetComponent<SphereCollider>();
		anim = GetComponentInChildren<Anim>();
		waypoints =  GameObject.FindGameObjectsWithTag("Shit");
	}
	void Start()
	{
		currentState = State.Idle;
	}
	void Update()
	{
		if (hunger >= 50) 
		{
			bHungry = true;
			currentState = State.Hungry;
			
		}else bHungry = false;
		count = qVisibleFood.Count;
		myState = currentState.ToString();
		if(AStar.TargetReached)
		{
			base.DoOnTargetReached();
		}
		base.DoUpdate ();
	}
	void PickRandomLocation()
	{
		float dx = Random.Range (-100, 100);
		float dz = Random.Range (-100, 100);
		dLocation = transform.position+ new Vector3 (dx, 0 ,dz);
		AStar.setTargetLocation (dLocation);
		bBusy = true;
	}

	void SearchWaypoints()
	{
		GameObject point = waypoints[Random.Range(0,waypoints.Length-1)];
		Vector2 circle = Random.insideUnitCircle * 20;
		Vector3 pos = point.transform.position + new Vector3 (circle.x, 0 ,circle.y);
		target = point.transform;
		bBusy = true;
		AStar.setTargetLocation(pos);
	}

	public void Bite()
	{
		anim.OnBite();
		Destroy(target.gameObject,0.73f);
		hunger -= 5;
		Debug.Log("Ass");
	}
	public Transform DequeueFood()
	{
		if(qVisibleFood.Count>0)
		{
			Transform food = (Transform)qVisibleFood.Dequeue(); 
			
			if (food!=null)
			{
				target = food;
				AStar.setTarget(target);
				bBusy = true;
				return food;
			}else return null;
		}else return null;
	}
	 
	#region Idle

	IEnumerator Idle_EnterState()
	{
		hunger+=.05f;
		yield return 3;
	}
	void Idle_Update()
	{
		if(bHungry)
		{
			currentState = State.Hungry;
		}else currentState = State.Moving;

	}
	#endregion
	#region Moving
	IEnumerator Moving_EnterState()
	{
		//hunger+=1f;
		SearchWaypoints();
		hunger+=.05f;
		yield return null;

	}
	void Moving_OnTargetReached()
	{
		bBusy = false;
		currentState = State.Idle;
	}
	#endregion

	#region Hungry

	IEnumerator Hungry_EnterState()
	{
		if(!bBusy)
		{
			if ((target==null||target.tag!="Food"))
			{
				if(qVisibleFood.Count>0)
				{
					Transform food = (Transform)qVisibleFood.Dequeue(); 

					if (food!=null)
					{
						Vector3 direction = transform.position - food.position;
						Debug.DrawRay(transform.position+ transform.up, direction, Color.red);
						target = food;
						AStar.setTarget(target);
						bBusy = true;
					}
				}else{
					SearchWaypoints(); 
				}
					
			}else{AStar.setTarget(target);bBusy = true;}
		}
		yield return new WaitForSeconds (5);
	}

	void Hungry_Update()
	{
		if (!bHungry)
		{
			currentState = State.Idle;
		}
		AStar.canMove = true;
		AStar.canSearch = true;
	}

	void Hungry_OnTargetReached()
	{
		//do I have a target?
		if (target!=null)
		{
			//What is this target?
			if (target.tag == "Food")
			{
				Bite ();
			}
			if (target.tag == "Shit")
			{
				SearchWaypoints();
			}
		}
		bBusy = false;
	}
	

	void Hungry_OnTriggerStay(Collider other)
	{
		Vector3 direction = other.transform.position - transform.position;
		float angle = Vector3.Angle(direction, transform.forward);
		//Is the object in my field of view?
		if(angle < fieldOfViewAngle*0.5f)
		{
			RaycastHit hit;
			//are ther any obstructions between me and the object?
			if(Physics.Raycast(transform.position + transform.up, direction.normalized, out hit, col.radius))
			{
				//what is the object?
				if(hit.collider.gameObject.tag == "Food")
				{
					Debug.DrawRay(transform.position+ transform.up, direction, Color.red);
					bFoodVisible = true;
					Debug.Log("Found IT!");
					if (!qVisibleFood.Contains(other.transform))
					{
						qVisibleFood.Enqueue(other.transform);
					}
				}
			}
		}
	}

	#endregion
*/
}
