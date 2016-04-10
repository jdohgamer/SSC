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
				if(row<boardWidth && row >=0 && col<boardLength && col>=0)
				{
					neighbors.Add(new Vector3(row,0,col)); //only adding valid board locations
				}
				negDistColumn++;//increment until we reach height
			}
			negDist++; //increment until we reach width
		}
	}
} 

public class Grid_Setup : MonoBehaviour 
{
	public static GameObject Ball;
	public static Grid_Setup Instance;//not technically a singleton
	public Team[] Teams;
	public Vector3 BallLocation{get{return Ball.transform.position;}}
	public int Length{get{return length;}}
	public int Width{get{return width;}}
	public int TeamSize{ get{return teamSize;}}
	[SerializeField] GameObject highlightFab, ballFab = null, charFab = null;
	[SerializeField] CharacterData[] positionData;
	[SerializeField] Color[] TeamColors =  {Color.black, Color.white};
	[SerializeField] int teamSize = 5;
	[SerializeField] Vector3 TeamOneGoal = Vector3.zero, TeamTwoGoal = Vector3.one, GoalSize = Vector3.one;
	private static Cell highlightSingle;
	private Cell[,] cells2D;
	private Transform fieldTran, TeamOneTran, TeamTwoTran;
	private AdjacentIndexes adjacent;
	public bool isHighlighted, isCreated;
	private int length, width, cellCount;

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
	public UnitController GetCharacter(int Team, int index)
	{
		return Teams [Team].mates [index];
	}

	public void SetCharacter(int team,int index, Vector3 location)
	{
		if(team<Teams.Length && index<teamSize)
		{
			Teams[team].mates[index].gameObject.SetActive(true);
			Teams[team].mates[index].MoveTransform(GetCellByLocation(location).Location);
		}
	}
	public void LoadCharactersFromProps(Hashtable ht)
	{
		for(int i = 0;i<ht.Count;i++)
		{
			Hashtable hash = ht[i.ToString()]as Hashtable;
			int team = (int)hash["Team"];
			Vector3 loc = (Vector3)hash["Location"];
			this.SetCharacter(team, i, loc);
		}
	}
	public void ResetBoard ()
	{
		Ball.GetComponent<BallScript>().StopMe();
		Ball.transform.SetParent (null);
		Ball.transform.position = new Vector3 (11,0.1f,6);
		Teams [0].Sleep ();
		Teams [1].Sleep ();
	}
	public bool IsInsideHighlighted(Vector3 spot)
	{
		if (isHighlighted)
			return adjacent.neighbors.Contains (spot);
		else
			return false;
	}
	public void Generate (int w, int l) 
	{
		if(!isCreated)
		{
			Teams = new Team[2];
			for(int t = 0; t<2 ; t++)
			{
				//bool teamOne = t == 0;
				Vector3 goal = t == 0 ? TeamOneGoal : TeamTwoGoal;
				Teams [t] = new Team ((Team.TeamNumber)t, TeamColors[t], teamSize, goal, GoalSize);
				//Quaternion face = teamOne ? Quaternion.LookRotation(Vector3.right):Quaternion.LookRotation(-Vector3.right) ;
				for(int c = 0; c <teamSize; c++)
				{
					GameObject newGuy = Instantiate(charFab,Vector3.zero + new Vector3((float)t,0.2f,(float)c),Quaternion.identity) as GameObject;
					Teams [t].AddMate(newGuy.GetComponent<UnitController>());
					Teams [t].mates [c].charData = positionData [c];
					newGuy.SetActive (false);
				}
			}

			isCreated = true;
			width = w+2;
			length = l+2;
			cellCount = length*width;
			cells2D = new Cell[width,length];
			GameObject high;
			Vector3 loc = Vector3.zero, size = new Vector3 (1,1,1);
			Team.TeamNumber fieldSide = Team.TeamNumber.NONE;
			int type = 0;
			int i = 0;
			for (int x = 0; x<width; x++)
			{
				for (int z = 0; z<length; z++)
				{	
					fieldSide = Team.TeamNumber.NONE;
					if(x==0||x==width-1||z==0||z==length-1)//Border
					{
						type = 0;
					}else{
						type = 3;
						if (x < (width / 2)) 
						{
							fieldSide = Team.TeamNumber.TeamOne;
						} else if (x > (width / 2))
						{
							fieldSide = Team.TeamNumber.TeamTwo;
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
	public bool IsShotOnGoal(int tNum, Vector3 spot)
	{
		return Teams[tNum].IsVectorInGoal (spot);
		
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
			}//else Debug.Log(v);
		}
		isHighlighted = true;
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
			int tempWidth = 21;    
			int tempLength = 11;
			if (customProps.ContainsKey("tx#"))
			{
				width = (int)customProps["tx#"];
				length = (int)customProps["tz#"];
			}
			this.Generate(tempWidth, tempLength);
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
