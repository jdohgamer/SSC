using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; 
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;

//struct AdjacentIndexes 
//{
//	public List<Cell> neighbors;
//	
//	public AdjacentIndexes(Vector3 index, int distance,  int boardLength, int boardWidth)
//	{
//		neighbors = new List<Cell>();
//		int dist = distance*2+1; //total width of square we look in
//		int negDist = -distance; //we want to itereate between the positive and negative value
//		int negDistColumn; //relative "z" value going "up", think of as typical "y"
//		int negDistRow; //relative "x" value going "right"
//		float row, col; //these will be the actual positions in our return Vector
//		for(int x = 0; x<dist; x++)
//		{
//			negDistRow = negDist; // our "x" value will change in this outer loop from -dist to dist (equaling width)
//			negDistColumn = -distance; //our "y" value change in the inner loop from -distance to disance (equaling height)
//			for(int z = 0; z<dist; z++)
//			{
//				row = negDistRow+index.x;
//				col = negDistColumn+index.z;
//				if(row<boardWidth && row >=0 && col<boardLength && col>=0)
//				{
//					neighbors.Add(Grid_Setup.Instance.GetCellByLocation( new Vector3(row,0,col))); //only adding valid board locations
//				}
//				negDistColumn++;//increment until we reach height
//			}
//			negDist++; //increment until we reach width
//		}
//	}
//} 

public class Grid_Setup : MonoBehaviour 
{
	public static GameObject Ball;
	public static Grid_Setup Instance;//not technically a singleton
	public bool isHighlighted, isCreated;
	public Vector3 BallLocation{get{return Ball.transform.position;}}
	[SerializeField] public Vector3 TeamOneGoal = Vector3.zero, TeamTwoGoal = Vector3.one, GoalSize = Vector3.one;
	[SerializeField] private GameObject highlightFab, ballFab = null;
	[SerializeField] private int  length = 13 , width = 23;
	private static Cell highlightSingle;
	private Cell[,] cells2D;
	private Transform fieldTran;
	//private AdjacentIndexes adjacent;
	private int cellCount, fieldSide;
	private List<Cell> neighbors;

	void Awake()
	{
		GameObject field = new GameObject("Field");
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

	public void ResetBoard ()
	{
		Ball.GetComponent<BallScript>().StopMe();
		Ball.transform.SetParent (null);
		Ball.transform.position = new Vector3 (11,0.1f,6);
		//Teams [0].Sleep ();
		//Teams [1].Sleep ();
	}
	public bool IsInsideHighlighted(Cell spot)
	{
		if (isHighlighted)
			return neighbors.Contains (spot);
		else
			return false;
	}
	public void Generate () 
	{
		if(!isCreated)
		{
			isCreated = true;
			cellCount = length*width;
			cells2D = new Cell[width,length];
			GameObject high;
			Vector3 loc = Vector3.zero, size = new Vector3 (1,1,1);
			fieldSide = -1;
			int type = 0;
			int i = 0;
			for (int x = 0; x<width; x++)
			{
				for (int z = 0; z<length; z++)
				{	
					fieldSide = -1;
					if(x==0||x==width-1||z==0||z==length-1)//Border
					{
						type = 0;
					}else{
						type = 3;
						if (x < (width / 2)) 
						{
							fieldSide = 0;
						} else if (x > (width / 2))
						{
							fieldSide = 1;
						}
						if(x==width/2&&z==length/2)
						{
							if(Ball==null)
							Ball = Instantiate(ballFab,new Vector3(x,0.1f,z), Quaternion.identity)as GameObject;
						}
					}
					loc.x = x;
					loc.z = z;
					cells2D[x,z] = new Cell(i,type,fieldSide,loc,size);
					high = Instantiate(highlightFab,loc,Quaternion.identity) as GameObject;
					high.transform.SetParent(fieldTran);//set position globally before parenting
					cells2D[x,z].SetHighlighter(high);
					i++;
				}
			}		
		}
	}
	
	public Cell GetCellByLocation(Vector3 locaton)
	{
		int x = (int)Mathf.Round(locaton.x);//we use a unit size of 1, elsewise multiply by size
		int z = (int)Mathf.Round(locaton.z);
		if(cells2D!=null&& cells2D[x,z]!=null)
		return cells2D[x,z];
		else return null;
	}
	public Cell GetCellByID(int id)
	{
		int row = (int)Mathf.Floor(id/length);//truncate to "ten's" place
		int col = (int)(id % length);//get the remainder of length
		if(cells2D[row,col]!=null)
		{
			return cells2D[row,col];
		}
		return null; 
	}
	public Cell GetNearestCellToDestination(Vector3 locFrom, Vector3 locTo)
	{
		Vector3 diff = locTo -locFrom;
		if(diff.x!=0)
		{diff.x = diff.x/Mathf.Abs(diff.x);}
		if(diff.z!=0)
		{diff.z = diff.z/Mathf.Abs(diff.z);}
		Cell newCTo = GetCellByLocation(locTo - diff);
		return newCTo;
	}

//	public void HighlightAdjacent(bool set, Vector3 index, int distance)//get rid of bool set, I never use it
//	{
//		if (isHighlighted||set==false)
//		{
//			TurnOffHiglightedAdjacent ();
//		}
//	
//		adjacent = new AdjacentIndexes (index, distance, this.length, this.width);
//		foreach(Vector3 v in adjacent.neighbors)
//		{
//			Cell c = GetCellByLocation(v);
//			if(c !=null && c.type!=Cell.CellType.OutOfBounds)
//			{
//				c.Highlight(true);
//			}//else Debug.Log(v);
//		}
//		isHighlighted = true;
//	}

	public void HighlightAdjacent( Predicate<Cell> predicate, Vector3 index, int distance)
	{
		if (isHighlighted)
		{
			TurnOffHiglightedAdjacent ();
		}

		neighbors = new List<Cell>();
		int dist = distance*2+1; //total width of square we look in
		int negDist = -distance; //we want to itereate between the positive and negative value
		int negDistColumn; //relative "z" value going "up", think of as typical "y"
		int negDistRow; //relative "x" value going "right"
		float row, col; //these will be the actual positions in our return Vector
		for(int x = 0; x<dist; x++)
		{
			negDistRow = negDist; // our "x" value will change in this outer loop from -dist to dist (equaling width)
			negDistColumn = -distance; //our "y" value change in the inner loop from -distance to disance (equaling height)
			for(int z = 0; z<dist; z++)
			{
				row = negDistRow+index.x;
				col = negDistColumn+index.z;
				if(row<width && row >=0 && col<length && col>=0)
				{
					Cell c = GetCellByLocation(new Vector3(row,0,col));
					if(predicate.Invoke(c))
					{
						c.Highlight(true);
						neighbors.Add(c);
					}
					//neighbors.Add(Grid_Setup.Instance.GetCellByLocation( new Vector3(row,0,col))); //only adding valid board locations
				}
				negDistColumn++;//increment until we reach height
			}
			negDist++; //increment until we reach width
		}
//		adjacent = new AdjacentIndexes (index, distance, this.length, this.width);
		isHighlighted = true;
	}
	public void TurnOffHiglightedAdjacent()
	{
		if (isHighlighted) 
		{
			foreach(Cell v in neighbors)
			{
				v.Highlight(false);
			}
			isHighlighted = false;
		}
	}
	
	public void HighlightSingle(Vector3 location)
	{
		if(highlightSingle!=null)
		{
			highlightSingle.Highlight(false);
		}
		highlightSingle = GetCellByLocation(location);
		highlightSingle.Highlight(true);
	}
	public void TurnOffSingle()
	{
		if(highlightSingle!=null)
		{
			highlightSingle.Highlight(false);
		}
	}
	
	protected internal Hashtable GetBoardAsCustomProperties()
	{
		Hashtable customProps = new Hashtable();
		for (int i = 0; i < cellCount; i++)
		{
			customProps[i.ToString()] = (int)GetCellByID(i).type;
		}
//		foreach(FSM_Character c in characters2D)
//		{
//			if(c!=null)
//			customProps.Add("character#"+c.id,c.GetCharacterAsProp());
//		}	
		
		return customProps;
	}
	
	protected internal bool SetBoardByCustomProperties(Hashtable customProps, bool calledByEvent)
	{ 
		if (!calledByEvent)
		{
			MainGame.Instance.NewGame();
		}

		int readTiles = 0;
		for (int i = 0; i < cellCount; i++)
		{
			if (customProps.ContainsKey(i.ToString()))
			{
				readTiles++;
			}
		}

		return true;//readTiles == cellCount;
	}

}
