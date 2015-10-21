using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BallScript : MonoBehaviour 
{
	public static Vector3 TargetLocation;
	[SerializeField] Vector3 offset =  new Vector3(0,0.1f,0);
	private Vector3 moveDirection;
	Rigidbody rigid;
	bool bKicked;

	void Awake()
	{
		rigid = GetComponent<Rigidbody> ();
	}
	public void StopMe()
	{
		rigid.velocity = Vector3.zero;
		rigid.isKinematic = true;
	}

	void FixedUpdate()
	{
		if (bKicked&&(rigid.velocity.magnitude <= 1f)) 
		{
			rigid.isKinematic = true;
			transform.position = Grid_Setup.Instance.GetCellByLocation (transform.position).Location + offset;
			bKicked = false;
		}
	}
	void OnCollisionEnter(Collision other)
	{
		if (other.collider.CompareTag ("Player")) 
		{
			if (rigid.isKinematic) 
			{
				Vector3 dir = transform.position - other.contacts [0].point;
				transform.Translate (dir.normalized);
			}
		}
	}

	public void BallisticVelocity(Vector3 target, float angle)
	{
		TargetLocation = target;
		moveDirection = target - transform.position;
		float height = moveDirection.y; // height difference
		moveDirection.y = 0; // keep only horizontal information
		float dist = moveDirection.magnitude;// horizontal distance
		float a = angle * Mathf.Deg2Rad;
		moveDirection.y = dist * Mathf.Tan (a);//correction for small height differences

		//calculate velocity magnitude
		float velocity = Mathf.Sqrt(dist*Physics.gravity.magnitude/Mathf.Sin(2*a));
		rigid.isKinematic = false;
		bKicked = true;
		rigid.velocity =  velocity * moveDirection.normalized;
	}
//	public IEnumerator MoveTo(Hashtable ht)
//	{
//		canMove = true;
//		targetCell = Grid_Setup.Instance.GetCellByID((int)ht["Cell"]);
//		moveSpeed = ((float)ht["Speed"])*5;
//		easeType = (string)ht["EaseType"];
//		targetLocation = targetCell.Location;
//		Vector3 dir = targetLocation - transform.position+offset;
//		dir = dir.normalized;
//		while (moveSpeed > 0.1f) 
//		{
//			rigid.MovePosition (transform.position + (dir * moveSpeed * Time.deltaTime));
//			moveSpeed = Mathf.Lerp (moveSpeed, 0, 0.1f);
//			yield return new WaitForSeconds(0.1f);
//		}
//		while (Vector3.Distance(transform.position,TargetLocation)>.1f && canMove) 
//		{
//			iTween.MoveAdd(gameObject, iTween.Hash("amount", dir, "easeType", easeType, "loopType", "none", "speed", moveSpeed));
//			yield return new WaitForSeconds(1f);
//		}
		//StopCoroutine("MoveTo");
	//}
}
