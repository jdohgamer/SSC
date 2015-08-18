using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using RAIN.Core;
using RAIN.Navigation.Waypoints;
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Grid_Setup : MonoBehaviour 
{
	public int length, width, cellCount;
	public GameObject[] boardCell;
	Cell[] cells;
	public GameObject ball;
<<<<<<< HEAD
	public GameObject PotMove;
	public Vector3 size;
	public Vector3 ballPosition;
	GameObject field;
	Transform fieldTran;
	GameObject cell;
=======
	GameObject field;
	Transform fieldTran;
>>>>>>> BTest

	/* Dirt = 0
	 * Corner Lines = 1
	 * Lines = 2
	 * Grass = 3
	 */
	private List <Vector3> gridPositions = new List <Vector3> ();

//	private WaypointRig wpRig = null;
//	WaypointSet tWPSet;
	
	void Awake()
	{
		cellCount = (width+3)*(length+3);
		field = new GameObject("Field");
		fieldTran = field.transform;
		cells = new Cell[cellCount];
//		wpRig = GetComponentInChildren<WaypointRig>();
//		tWPSet = wpRig.WaypointSet;
	}

	void Start()
	{

		//Generate();
	}

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

	public void Generate () 
	{
		int i = 0;
		for (int x = -1; x<=width+1; x++)
		{
			for (int z = -1; z<=length+1; z++)
			{
				int type = 0;
				float rot = 0f;

				if(x ==-1||x ==width+1||z ==-1|| z == length+1)//OutOfBounds
				{
					type = 0; rot = 0f;
				}else if(x==0||x==width||z==0||z==length)//Border
				{
					if(x==0) //left side
					{
						if(z==0)		{  type = 1; rot = 90f;} //bottom left
						else if(z==length){type = 1; rot = 0f;;} //top left
						else{			   type = 2; rot = 90f;} //if(z>0&&z<length)
					}
					else if(x==width)//right side
					{
						if(z==0){		   type = 1; rot = 180f;} //bottom right
						else if(z==length){type = 1; rot = -90f;} //top right
						else {			   type = 2; rot = 90f;} //if(z>0&&z<length)
					}else{ //if(x>0&&x<width&&z==0||z==length)
						type = 2; rot = 0f;;
					}
				}else { //inner field
					type = 3; rot = 0f;
					if(x==width/2&&z==length/2)
					{
						GameObject newBall = Instantiate(ball,new Vector3(x,0.2f,z), Quaternion.identity)as GameObject;

					}
				}
				cells[i] = new Cell(i,type);
				cells[i].boardObj = CreateCell(type,x,z,rot);
				cells[i].cm = cells[i].boardObj.GetComponent<CellMono>();
				i++;
			}
		}
	}
	GameObject CreateCell(int type,int x, int z, float rotation)
	{
<<<<<<< HEAD
		//GameObject cell;
		cell = Instantiate(boardCell[type],new Vector3(x,0,z), Quaternion.identity)as GameObject;
		cell.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
=======

		GameObject cell = Instantiate(boardCell[type],new Vector3(x,0,z), Quaternion.identity)as GameObject;
		//cell.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
>>>>>>> BTest
		cell.transform.SetParent (fieldTran);
		if(cell.GetComponent<Renderer>().material.GetFloat("_RotationDegree")!= null)
		{
			cell.GetComponent<Renderer>().material.SetFloat("_RotationDegree", rotation* Mathf.Deg2Rad);
		}
		return cell;
		//tWPSet.AddWaypoint(new Waypoint(cell.transform.position));

	}

	protected internal Hashtable GetBoardAsCustomProperties()
	{
<<<<<<< HEAD
		field = new GameObject("Field");
		fieldTran = field.transform;
		Generate();
		ballPosition = ball.transform.position;

	}
	void Update () 
	{


	}
	void OnMouseOver()
	{
=======
		Hashtable customProps = new Hashtable();
		for (int i = 0; i < cellCount; i++)
		{
			customProps[i.ToString()] = (int)cells[i].type;
		}

		return customProps;
	}
>>>>>>> BTest
	
	protected internal bool SetBoardByCustomProperties(Hashtable customProps, bool calledByEvent)
	{
		if (!calledByEvent)
		{
			this.width = 15;    // original game had 4x4
			this.length = 10;
			if (customProps.ContainsKey("tx#"))
			{
				this.width = (int)customProps["tx#"];
				this.length = (int)customProps["tz#"];
			}
			
			this.Generate();
		}
		
		int readTiles = 0;
		for (int i = 0; i < cellCount; i++)
		{
			if (customProps.ContainsKey(i.ToString()))
			{
				readTiles++;
			}
		}

		return readTiles == cellCount;
	}

}
	
