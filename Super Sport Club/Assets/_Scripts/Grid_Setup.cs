using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;

struct AdjacentIndexes 
{
	public List<Vector3> neighbors;
	
	public AdjacentIndexes(Vector3 index, int distance,  int boardLength, int boardWidth)
	{
		neighbors = new List<Vector3>();
		int dist = distance*2+1;
		int negDist = -distance;
		int negDistColumn;
		int negDistRow;
		float row, col;
		for(int x = 0; x<dist; x++)
		{
			negDistRow = negDist ;
			negDistColumn = -distance;
			for(int z = 0; z<dist; z++)
			{
				row = negDistRow+index.x;
				col = negDistColumn+index.z;
				if(row<boardWidth && row >=0 && col<boardLength && col>=0)
				{
					neighbors.Add(new Vector3(row,0,col));
				}
				negDistColumn++;
			}
			negDist++;
		}
	}
} 

public class Grid_Setup : MonoBehaviour 
{
	public static GameObject Ball;
	public static Grid_Setup Instance;
	[HideInInspector]public FSM_Character[] characters;
	[HideInInspector]public Cell[,] cells2D;
	[HideInInspector]public int length, width, cellCount;
	[SerializeField] GameObject highlight;
	[SerializeField] GameObject ball;
	static Cell highlightSingle;
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
			isCreated = true;
			width = w+3;
			length = l+3;
			cellCount = length*width;
			cells2D = new Cell[width,length];
			GameObject high;
			Vector3 loc = Vector3.zero, size = new Vector3 (1,0,1);
			int type = 0;
			int i = 0;
			for (int x = 0; x<width; x++)
			{
				for (int z = 0; z<length; z++)
				{					
					if(x==0||x==width-1||z==0||z==length-1)//Border
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
	}
	
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
		{
			TurnOffHiglightedAdjacent ();
		}
	
		adjacent = new AdjacentIndexes (index, distance, this.length, this.width);
		foreach(Vector3 v in adjacent.neighbors)
		{
			Cell c = GetCellByLocation(v);
			if(c !=null && c.type!=Cell.CellType.OutOfBounds)
			{
				c.Highlight(true);
			}else Debug.Log(v);
		}
		isHighlighted = true;
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
	public void TurnOffHiglightedAdjacent()
	{
		if (isHighlighted) 
		{
			foreach(Vector3 v in adjacent.neighbors)
			{
				GetCellByLocation(v).Highlight(false);
			}
			isHighlighted = false;
		}
	}
	
	protected internal Hashtable GetBoardAsCustomProperties()
	{
		Hashtable customProps = new Hashtable();
		for (int i = 0; i < cellCount; i++)
		{
			customProps[i.ToString()] = (int)GetCellByID(i).type;
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
