using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using RAIN.Core;
using RAIN.Navigation.Waypoints;
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;


struct AdjacentIndexes 
{
	//public int[] indexes;
	public int[,] indexes2D;

	public AdjacentIndexes(int index, int distance,  int boardLength)
	{
		int dist = distance*2+1;
		int negDist = -distance;
		indexes2D = new int[dist,dist];
		int negDistColumn;
		int negDistRow;
		for(int x = 0; x<dist; x++)
		{
			negDistRow = negDist * boardLength;
			negDistColumn = -distance;
			for(int y = 0; y<dist; y++)
			{
				indexes2D[x,y] = negDistRow + negDistColumn + index;
				negDistColumn++;
			}
			negDist++;
		}
	}
} 

public class Grid_Setup : MonoBehaviour 
{
	[SerializeField]  GameObject[] boardCell;
	[SerializeField] GameObject ball;
	public static GameObject Ball;
	public FSM_Character[] characters;
	public Cell[] cells;
	public int length, width, cellCount;
	GameObject field;
	Transform fieldTran;
	AdjacentIndexes adjacent;
	bool isHighlighted;

	/* Dirt = 0
	 * Corner Lines = 1
	 * Lines = 2
	 * Grass = 3
	 */
		
	void Awake()
	{
		field = new GameObject("Field");
		fieldTran = field.transform;
	}

	void DestroyBoard ()
	{
		for (int i=0; i<cellCount; i++) 
		{
			Destroy(cells[i].boardObj);
		}
		Destroy (Ball);
	}
	public void Generate (int w, int l) 
	{
		DestroyBoard ();
		width = w;
		length = l;
		int i = 0;
		cellCount = (w+3)*(l+3);//
		cells = new Cell[cellCount];
		for (int x = -1; x<=w+1; x++)//this is actually one unit too long, buy I don't feel like changinging it
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
						else{			   type = 2; rot = -90f;} //if(z>0&&z<length)
					}
					else if(x==w)//right side
					{
						if(z==0){		   type = 1; rot = 180f;} //bottom right
						else if(z==l){type = 1; rot = -90f;} //top right
						else {			   type = 2; rot = 90f;} //if(z>0&&z<length)
					}else{ //if(x>0&&x<width
						if(z==0)
						{
							type = 2; rot = 0;
						}else if (z==length)
						{
							type = 2; rot = 180f;
						}
					}
				}else { //inner field
					type = 3; rot = 0f;
					if(x==w/2&&z==l/2)
					{
						Ball = Instantiate(ball,new Vector3(x,0.2f,z), Quaternion.identity)as GameObject;
					}
				}
				cells[i] = new Cell(i,type);
				cells[i].boardObj = CreateCell(type,x,z,rot);
				cells[i].cm = cells[i].boardObj.GetComponent<CellMono>();
				i++;
			}
		}
		for (int c = 0; c<characters.Length; c++) 
		{
			characters[c].OccupiedCell = cells[characters[c].RaycastToGround()];
			characters[c].OccupiedCell.character = characters[c];
		}
	}
	GameObject CreateCell(int type,int x, int z, float rotation)
	{
		GameObject cell = Instantiate(boardCell[type],new Vector3(x,0,z), Quaternion.identity)as GameObject;
		cell.transform.SetParent (fieldTran);
		if(cell.GetComponent<Renderer>().material.GetFloat("_RotationDegree")!= null)
		{
			cell.GetComponent<Renderer>().material.SetFloat("_RotationDegree", rotation* Mathf.Deg2Rad);
		}
		return cell;
	}

	public void HighlightAdjacent(bool set, int index, int distance)
	{
		if (isHighlighted||set==false)
			TurnOffHiglighted ();
	
			adjacent = new AdjacentIndexes (index, distance, length + 3);
			int dist = distance * 2 + 1;
			for (int h =0; h<dist; h++) 
			{
				for (int i =0; i<dist; i++) 
				{
					if (adjacent.indexes2D [h, i] >= 0 && adjacent.indexes2D [h, i] != index) 
					{
						if (cells [adjacent.indexes2D [h, i]].type != Cell.CellType.OutOfBounds) 
						{
							cells [adjacent.indexes2D [h, i]].cm.Highlight (set);
						}
					}
				}
			}
			isHighlighted = true;

	}
	public void TurnOffHiglighted()
	{
		if (isHighlighted) 
		{
			foreach (int i in adjacent.indexes2D) {
				cells [i].cm.Highlight (false);
			}
			isHighlighted = true;
		}
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
			
			this.Generate(width, length);
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
