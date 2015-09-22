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
	public static Grid_Setup Instance;
	public FSM_Character[] characters;
	public FSM_Character[,] characters2D;
	[HideInInspector]public Cell[,] cells2D;
	public int length, width, cellCount;
	[SerializeField] GameObject highlight;
	[SerializeField] GameObject ball;
	[SerializeField] GameObject charFab;
	[SerializeField] CharacterData[] positionData;
	static Cell highlightSingle;
	GameObject field;
	Transform fieldTran;
	AdjacentIndexes adjacent;
	bool isHighlighted, isCreated;
	int characterCount = 0, maxCharacters = 10, teamSize = 5, teamOneSize, teamTwoSize;

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
	public FSM_Character GetCharacter(int Team, int index)
	{
		return characters2D[Team,index];
	}
	public FSM_Character GetCharacter(int index)
	{
		if(index>=teamSize)
		return characters2D[1,index%teamSize];
		else
		return characters2D[0,index];
	}
	public FSM_Character AddCharacter(int Team,int index, Vector3 location, string playPosition)
	{
		if(characterCount<maxCharacters)
		{
			GameObject newGuy = Instantiate(charFab,GetCellByLocation(location).Location + new Vector3(0,0.2f,0),Quaternion.identity) as GameObject;
			//characters[characterCount] =  newGuy.GetComponent<FSM_Character>();
			//characters[characterCount].id = characterCount;
			characters2D[Team,index] =  newGuy.GetComponent<FSM_Character>();
			characters2D[Team,index].id = index;
			characters2D[Team,index].team = (CustomGameClient.Team)Team;
			foreach(CharacterData cd in positionData)
			{
				if(cd.name == playPosition)
				{
					characters2D[Team,index].charData = cd;
					break;
				}
			}
			characterCount++;
			return characters2D[Team,index];
		}else return null;
	}
	public void Generate (int w, int l) 
	{
		if(!isCreated)
		{
			characters = new FSM_Character[maxCharacters];
			characters2D = new FSM_Character[2,teamSize];
			isCreated = true;
			width = w+2;
			length = l+2;
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
//			for (int c = 0; c<characters.Length; c++) 
//			{
//				characters[c].OccupiedCell = GetCellByLocation(characters[c].Location);
//				characters[c].OccupiedCell.character = characters[c];
//			}
		}
	}
	
	public Cell GetCellByLocation(Vector3 locaton)
	{
		int x = (int)Mathf.Round(locaton.x);
		int z = (int)Mathf.Round(locaton.z);
		if(cells2D!=null&& cells2D[x,z]!=null)
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
