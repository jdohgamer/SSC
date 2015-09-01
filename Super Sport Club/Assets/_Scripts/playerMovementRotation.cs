using UnityEngine;
using System.Collections;

public class playerMovementRotation : MonoBehaviour 
{
	public float speed = 10;
	public static Vector3 movement;
	private Animator anim;
	private Transform tran;
	void Start () 
	{
		anim = gameObject.GetComponent<Animator> ();
		tran = gameObject.GetComponent<Transform> ();
	}

	void Update() 
	{
			
		float lastInputX = Input.GetAxis ("Horizontal");
		float lastInputY = Input.GetAxis ("Vertical");

		movement = new Vector3 	(speed * lastInputX, speed * lastInputY, 0);
		Vector3 targetDir  = tran.position + movement*Time.deltaTime; 
		tran.position = Vector3.MoveTowards(tran.position,targetDir,0.25f);

		var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		//Debug.DrawRay(tran.position, movement, Color.blue);
		Quaternion rot = Quaternion.LookRotation (tran.position - mousePosition, Vector3.forward);
		tran.rotation = rot;
		tran.eulerAngles = new Vector3 (0, 0, tran.eulerAngles.z);
		GetComponent<Rigidbody2D>().angularVelocity = 0;
			
		if (lastInputX != 0 || lastInputY != 0) 
		{
			anim.SetBool ("Walking", true);
		} else 
		{
			anim.SetBool ("Walking", false);
			
		}
	}

}

			
	
