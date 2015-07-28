using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

public class Grid_Setup : MonoBehaviour 
{
	public int length, width;
	public GameObject[] boardCell;
	public GameObject ball, field;
	Transform fieldTran;
	/* Dirt = 0
	 * Corner Lines = 1
	 * Lines = 2
	 * Grass = 3
	 */
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
		for (int x = -1; x<=width+1; x++)
		{
			for (int z = -1; z<=length+1; z++)
			{
				if(x ==-1||x ==width+1||z ==-1|| z == length+1)
				{
					CreateCell(0,x,z,0f);
				}else if(x==0||x==width||z==0||z==length)
				{
					if(x==0) //left side
					{
						if(z==0){CreateCell(1,x,z,-90);} //bottom left
						else if(z==length){CreateCell(1,x,z,0f);} //top left
						else{CreateCell(2,x,z,90);} //if(z>0&&z<length)
					}
					else if(x==width)//right side
					{
						if(z==0){CreateCell(1,x,z,180f);} //bottom right
						else if(z==length){CreateCell(1,x,z,90);} //top right
						else {CreateCell(2,x,z,90);} //if(z>0&&z<length)
					}else{ //if(x>0&&x<width&&z==0||z==length)
						CreateCell(2,x,z,0f);
					}
				}else { //inner field
					CreateCell(3,x,z,0f);
					if(x==width/2&&z==length/2)
					{
						GameObject newBall = Instantiate(ball,new Vector3(x,0.2f,z), Quaternion.identity)as GameObject;
					}
				}

			}
		}
	}
	void CreateCell(int type,int x, int z, float rotation)
	{
		GameObject cell;
		cell = Instantiate(boardCell[type],new Vector3(x,0,z), Quaternion.identity)as GameObject;
		cell.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
		cell.transform.SetParent (fieldTran);
	}
	
	void Start()
	{
		field = new GameObject("Field");
		fieldTran = field.transform;
		Generate();
	}
	void Update () 
	{
	
	}
}
