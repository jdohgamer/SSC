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
	public Cell[] cells;
	public GameObject ball;
	GameObject field;
	public Transform fieldTran;

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

		field = new GameObject("Field");
		fieldTran = field.transform;

//		wpRig = GetComponentInChildren<WaypointRig>();
//		tWPSet = wpRig.WaypointSet;
	}

	public void Generate (int w, int l) 
	{
		width = w;
		length = l;
		int i = 0;
		cellCount = 299;//(w+3)*(l+3);
		cells = new Cell[cellCount];
		for (int x = -1; x<=w+1; x++)
		{
			for (int z = -1; z<=l+1; z++)
			{
				int type = 0;
				float rot = 0f;

				if(x ==-1||x ==w+1||z ==-1|| z == l+1)//OutOfBounds
				{
					type = 0; rot = 0f;
				}else if(x==0||x==w||z==0||z==l)//Border
				{
					if(x==0) //left side
					{
						if(z==0)		{  type = 1; rot = 90f;} //bottom left
						else if(z==l){type = 1; rot = 0f;;} //top left
						else{			   type = 2; rot = 90f;} //if(z>0&&z<length)
					}
					else if(x==w)//right side
					{
						if(z==0){		   type = 1; rot = 180f;} //bottom right
						else if(z==l){type = 1; rot = -90f;} //top right
						else {			   type = 2; rot = 90f;} //if(z>0&&z<length)
					}else{ //if(x>0&&x<width&&z==0||z==length)
						type = 2; rot = 0f;;
					}
				}else { //inner field
					type = 3; rot = 0f;
					if(x==w/2&&z==l/2)
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

		GameObject cell = Instantiate(boardCell[type],new Vector3(x,0,z), Quaternion.identity)as GameObject;
		//cell.GetComponent<Renderer>().material.shader = Shader.Find("Custon/RotateUVs");
		//cell.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
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
		Hashtable customProps = new Hashtable();
		for (int i = 0; i < cellCount; i++)
		{
			customProps[i.ToString()] = (int)cells[i].type;
		}

		return customProps;
	}
	
	protected internal bool SetBoardByCustomProperties(Hashtable customProps, bool calledByEvent)
	{
		if (!calledByEvent)
		{
			width = 15;    
			length = 10;
			if (customProps.ContainsKey("tx#"))
			{
				width = (int)customProps["tx#"];
				length = (int)customProps["tz#"];
			}
			
			//this.Generate(width, length);
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
