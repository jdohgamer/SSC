using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Gauge : MonoBehaviour 
{
	[SerializeField] Transform arrow;
	public float rotationDegreesPerSecond = 45f;
	public float degree, min, max;
	public float currentAngle;
	RectTransform tran;
	Quaternion temp;
	
	void Start () 
	{
		transform.eulerAngles += new Vector3(90,0,0);
		min = transform.eulerAngles.y-45;
		if(min<0)
		min = 360 + min;
		max = (transform.eulerAngles.y +225)%360;
		degree = min;
		Quaternion rot = Quaternion.LookRotation(transform.up);
		rot.eulerAngles += new Vector3(90,0,0);
		arrow.rotation = rot;
		
		currentAngle = arrow.rotation.eulerAngles.y;
	}
	
//	public void RotateTo(Vector3 dir)
//	{
//	
//	}

	void Update () 
	{
		currentAngle = arrow.rotation.eulerAngles.y;
		
		if(Mathf.Floor(currentAngle) == Mathf.Floor(min))
		{
			degree = max;
			//rotationDir = rotationDegreesPerSecond;

		}
		if(Mathf.Floor( currentAngle) == Mathf.Floor(max))
		{
			degree = min;
			//rotationDir = -rotationDegreesPerSecond;
		}
		temp = Quaternion.AngleAxis(degree,Vector3.up);
		//temp = Quaternion.LookRotation(arrow.right);
		temp.eulerAngles += new Vector3(90,0,0);
		arrow.rotation = Quaternion.RotateTowards(arrow.rotation, temp, rotationDegreesPerSecond*Time.deltaTime);
		
	}

	
}
