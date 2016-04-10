using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Gauge : MonoBehaviour 
{
	[SerializeField] Transform arrow = null, idealBar = null;
	[SerializeField] float rotationDegreesPerSecond = 45f;
	float degree, min, max;
	float currentAngle;
	Quaternion temp;
	bool run;
	
	void Start () 
	{
		transform.eulerAngles += new Vector3(90,0,0);
		min = Mathf.Floor(transform.eulerAngles.y-45);
		if(min<0)
		min = 360 + min;
		max = Mathf.Floor((transform.eulerAngles.y +225)%360);
		degree = min;
		Quaternion rot = Quaternion.LookRotation(transform.up);
		rot.eulerAngles += new Vector3(90,0,45);
		arrow.rotation = rot;
		currentAngle = arrow.rotation.eulerAngles.y;
		StartCoroutine(Bounce());
		run =true;
	}
	
	public void SetIdeal(Vector3 dir)
	{
		float ang = Vector3.Angle(transform.right,dir);
		idealBar.localEulerAngles = new Vector3(0,0,ang);
	}

	public Vector3 StopBounce()
	{
		run = false;
		//currentAngle = Mathf.Floor(arrow.rotation.eulerAngles.y);
		return arrow.right;
	}
	void Update () 
	{
		if(run)
		{
			currentAngle = Mathf.Floor(arrow.rotation.eulerAngles.y);
			if(currentAngle == min)
			{
				degree = max;
			}else
				if(currentAngle == max)
			{
				degree = min;
			}
			temp = Quaternion.AngleAxis(degree,Vector3.up);
			temp.eulerAngles += new Vector3(90,0,0);
			arrow.rotation = Quaternion.RotateTowards(arrow.rotation, temp, rotationDegreesPerSecond*Time.deltaTime);
			Debug.DrawRay(transform.position,arrow.right*5);
		}
	}
	
	IEnumerator Bounce()
	{
		
		yield return null;
	}
}
