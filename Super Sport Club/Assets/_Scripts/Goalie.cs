using UnityEngine;
using System.Collections;

public class Goalie : MonoBehaviour {

	public int currentShooterScore;
	public int shooterScore;
	public GameObject goalie;
	public GameObject shooter;
	private bool shotOnGoal;
	private bool kickDirectionLeft;
	private bool kickDirectionRight;
	private bool saveDirectionLeft;
	private bool saveDirectionRight;
	private Grid_Setup gs;
	public Camera main;
	public Camera cam_shot;



	// Use this for initialization
	void Start () {
		shotOnGoal = false;
		shooterScore = 0;






	
	}
	
	// Update is called once per frame
	void Update () {

		//if (kickDirectionLeft == saveDirectionLeft)
		
			//Debug.Log ("DENIED BITCH!");
		
		 if (kickDirectionLeft != saveDirectionLeft)
		
			shooterScore++;

	
	}


	void SwitchCam () 
	{

		//Application.LoadLevel("ShotOnGoal");
//		need to figure out how to carry over board data with scene cahnge
//		GameObject.DontDestroyOnLoad();
		main.enabled = false;
		cam_shot.enabled = true;

	}

	void OnTriggerEnter (Collider col) 
	{
		Debug.Log (col);
		if (col.gameObject.tag == "Ball")
		{
			Debug.Log ("1v1 IRL BRO?!");
			SwitchCam();
		}



	}



}	