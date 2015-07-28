using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Grid_Setup : MonoBehaviour 
{
	public int length, width;
	public GameObject[] boardCell;

	private Transform boardHolder;
	private List <Vector3> gridPositions = new List <Vector3> ();

	void InitialiseList ()
	{
		//Clear our list gridPositions.
		gridPositions.Clear ();
		
		//Loop through x axis (columns).
		for(int x = 1; x < width-1; x++)
		{
			//Within each column, loop through y axis (rows).
			for(int z = 1; z < length-1; z++)
			{
				//At each index add a new Vector3 to our list with the x and y coordinates of that position.
				gridPositions.Add (new Vector3(x, 0f, z));
			}
		}
	}

	void Generate () 
	{
		Quaternion turnRotation = Quaternion.LookRotation(new Vector3(0,0,-90));

		GameObject cell;

		for (int x = -1; x<width+1; x++)
		{
			for (int z = -1; z<length+1; z++)
			{
				if(x ==-1||x ==width||z ==-1|| z == length)
				{
					cell = Instantiate(boardCell[0],new Vector3(x,0,z), Quaternion.identity) as GameObject;
				}else if(x ==0||x == width-1||z ==0|| z == length-1){
					if(z ==0|| z == length-1)
					{
						if(x ==0&&z ==0)
						{
							cell = Instantiate(boardCell[2],new Vector3(x,0,z), Quaternion.identity)as GameObject;
							cell.transform.rotation = Quaternion.AngleAxis(-90, Vector3.up);
						}else
						{
							cell = Instantiate(boardCell[2],new Vector3(x,0,z), Quaternion.identity)as GameObject;
						}
					}else if(x ==0||x == width-1){
						cell = Instantiate(boardCell[1],new Vector3(x,0,z), Quaternion.identity)as GameObject;
						cell.transform.rotation = Quaternion.AngleAxis(90, Vector3.up);
					}

				}else {
					cell = Instantiate(boardCell[3],new Vector3(x,0,z), Quaternion.identity)as GameObject;
				}

			}
		}
	}
	
	void Start()
	{
		Generate();
	}
	void Update () 
	{
	
	}
}
