using UnityEngine;
using System.Collections;

public class LeftRight : MonoBehaviour {

	public float speed;
	Transform tran;
	// Use this for initialization
	void Start () 
	{
		tran = GetComponent<Transform> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		float h = Input.GetAxisRaw ("Horizontal");
		if (h != 0) 
		{
			Vector3 targetDir = new Vector3 (speed * h, 0, 0);
			targetDir  = tran.position + targetDir *Time.deltaTime; 
			tran.position = Vector3.MoveTowards(tran.position,targetDir,0.25f);
		}
	}
}
