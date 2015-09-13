using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;


struct AdjacentIndexes 
{
	//public int[,] indexes2D;
	public List<Vector3> neighbors;
	//public Vector3[] indexes;
	
	public AdjacentIndexes(Vector3 index, int distance,  int boardLength, int boardWidth)
	{
		neighbors = new List<Vector3>();
		int dist = distance*2+1;
		int negDist = -distance;
		//indexes2D = new int[dist,dist];
		//indexes = new Vector3[dist*dist];
		int negDistColumn;
		int negDistRow;
		float row, col;
		//int i=0;
		for(int x = 0; x<dist; x++)
		{
			negDistRow = negDist ;
			negDistColumn = -distance;
			for(int z = 0; z<dist; z++)
			{
				//indexes2D[x,y] = negDistRow + negDistColumn + index;
				//indexes[i] = new Vector3(negDistRow,0,negDistColumn)+ index;
				row = negDistRow+index.x;
				col = negDistColumn+index.z;
				if(row<boardWidth && row >=0 && col<boardLength && col>=0)
				{
					neighbors.Add(new Vector3(row,0,col));
				}
				//i++;
				negDistColumn++;
			}
			negDist++;
		}
	}
} 

public class Grid_Setup : MonoBehaviour 
{
	public static GameObject Ball;
	[HideInInspector]public FSM_Character[] characters;
	[HideInInspector]public Cell[] cells;
	[HideInInspector]public Cell[,] cells2D;
	[HideInInspector]public int length, width, cellCount;
	[SerializeField]  GameObject[] boardCell;
	[SerializeField] GameObject highlight;
	/* Dirt = 0
	 * Corner Lines = 1
	 * Lines = 2
	 * Grass = 3
	 */
	[SerializeField] GameObject ball;
	GameObject field;
	Transform fieldTran;
	AdjacentIndexes adjacent;
	bool isHighlighted, isCreated;
		
	void Awake()
	{
		field = new GameObject("Field");
		fieldTran = field.transform;
	}

	void DestroyBoard ()
	{
		for (int x = 0; x<(width+2); x++) 
		{
			for (int z = 0; z<(length+2); z++)
			{
				cells2D[x,z].DestroyHighlighter();
			}
		}
		Destroy (Ball);
	}
	public void Generate (int w, int l) 
	{
		if(!isCreated)
		{
			//DestroyBoard ();
			isCreated = true;
			GameObject high;
			width = w+3;
			length = l+3;
			int i = 0;
			cellCount = (w+2)*(l+2);//
			//cells = new Cell[cellCount];
			cells2D = new Cell[width,length];
			Vector3 loc = Vector3.zero, size = new Vector3 (1,0,1);
			int type = 0;
			for (int x = 0; x<width; x++)
			{
				for (int z = 0; z<length; z++)
				{
//					if(x ==-1||x ==w+1||z ==-1|| z == l+1)//OutOfBounds
//					{
//						type = 0;
//					}else 
					if(x==0||x==width||z==0||z==length)//Border
					{
						type = 0;
					}else{
						type = 3;
						if(x==width/2&&z==length/2)
						{
							if(Ball==null)
							Ball = Instantiate(ball,new Vector3(x,0.1f,z), Quaternion.identity)as GameObject;
							Ball.GetComponent<BallScript>().board = this;
						}
					}
					loc.x = x;
					loc.z = z;
					cells2D[x,z] = new Cell(i,type,loc, size);
					high = Instantiate(highlight,loc,Quaternion.identity) as GameObject;
					high.transform.SetParent(fieldTran);
					cells2D[x,z].SetHighlighter(high);
					i++;
				}
			}
			for (int c = 0; c<characters.Length; c++) 
			{
				//characters[c].OccupiedCell = GetCellByLocation(characters[c].Location);
				characters[c].OccupiedCell.character = characters[c];
			}
		}
//		for (int x = -1; x<=w+1; x++)//this is actually one unit too long, buy I don't feel like changinging it
//		{
//			for (int z = -1; z<=l+1; z++)
//			{
//				type = 0;
//				float rot = 0f;
//
//				if(x ==-1||x ==w+1||z ==-1|| z == l+1)//OutOfBounds
//				{
//					type = 0; rot = 0f;
//				}else if(x==0||x==w||z==0||z==l)//Border
//				{
//					if(x==0) //left side
//					{
//						if(z==0)		{  type = 1; rot = 90f;} //bottom left
//						else if(z==l){type = 1; rot = 0f;;} //top left
//						else{			   type = 2; rot = -90f;} //if(z>0&&z<length)
//					}
//					else if(x==w)//right side
//					{
//						if(z==0){		   type = 1; rot = 180f;} //bottom right
//						else if(z==l){type = 1; rot = -90f;} //top right
//						else {			   type = 2; rot = 90f;} //if(z>0&&z<length)
//					}else{ //if(x>0&&x<width
//						if(z==0)
//						{
//							type = 2; rot = 0;
//						}else if (z==length)
//						{
//							type = 2; rot = 180f;
//						}
//					}
//				}else { //inner field
//					type = 3; rot = 0f;
//					if(x==w/2&&z==l/2)
//					{
//						Ball = Instantiate(ball,new Vector3(x,0.1f,z), Quaternion.identity)as GameObject;
//						Ball.GetComponent<BallScript>().board = this;
//					}
//				}
//				cells[i] = new Cell(i,type);
//				cells[i].boardObj = CreateCell(type,x,z,rot);
//				cells[i].cm = cells[i].boardObj.GetComponent<CellMono>();
//				i++;
//			}
//		}
		
	}
//	GameObject CreateCell(int type,int x, int z, float rotation)
//	{
//		GameObject cell = Instantiate(boardCell[type],new Vector3(x,0,z), Quaternion.identity)as GameObject;
//		cell.transform.SetParent (fieldTran);
//		if(cell.GetComponent<Renderer>().material.GetFloat("_RotationDegree")!= null)
//		{
//			cell.GetComponent<Renderer>().material.SetFloat("_RotationDegree", rotation* Mathf.Deg2Rad);
//		}
//		return cell;
//	}

	public Cell GetCellByLocation(Vector3 locaton)
	{
		int x = (int)Mathf.Round(locaton.x);
		int z = (int)Mathf.Round(locaton.z);
		if(cells2D[x,z]!=null)
		return cells2D[x,z];
		else return null;
	}
	public Cell GetCellByID(int id)
	{
		int row = (int)Mathf.Floor(id/length);
		int col = (int)(id % length);
		if(cells2D[row,col]!=null)
		{
			return cells2D[row,col];
		}
		return null; 
	}

	public void HighlightAdjacent(bool set, Vector3 index, int distance)
	{
		if (isHighlighted||set==false)
			TurnOffHiglighted ();
	
			adjacent = new AdjacentIndexes (index, distance, this.length, this.width);
			foreach(Vector3 v in adjacent.neighbors)
			{
				if(GetCellByLocation(v)!=null)
				{
					GetCellByLocation(v).Highlight(true);
				}else Debug.Log(v);
			}
			isHighlighted = true;
		//			int dist = distance * 2 + 1;
//			for (int h =0; h<dist; h++) 
//			{
//				for (int i =0; i<dist; i++) 
//				{
//					if (adjacent.indexes2D [h, i] >= 0 && adjacent.indexes2D [h, i] < cellCount &&adjacent.indexes2D [h, i] != index) 
//					{
//						Cell c = GetCellByID(adjacent.indexes2D [h, i]);
//						//cells2D[index+
//						if (c.type != Cell.CellType.OutOfBounds) 
//						{
//							c.Highlight(true);
//							//GameObject high = Instantiate(highlight,c.Location+Vector3.up*.02f,Quaternion.identity) as GameObject;
//						}
//					}
//				}
//			}
			

	}
	public void TurnOffHiglighted()
	{
		if (isHighlighted) 
		{
			foreach(Vector3 v in adjacent.neighbors)
			{
				GetCellByLocation(v).Highlight(false);
			}
			isHighlighted = true;
		}
	}
	
	protected internal Hashtable GetBoardAsCustomProperties()
	{
		Hashtable customProps = new Hashtable();
		for (int i = 0; i < cellCount; i++)
		{
			customProps[i.ToString()] = 3;//(int)GetCellByID(i).type;
		}

		return customProps;
	}
	
	protected internal bool SetBoardByCustomProperties(Hashtable customProps, bool calledByEvent)
	{
		if (!calledByEvent)
		{
			width = 20;    
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
