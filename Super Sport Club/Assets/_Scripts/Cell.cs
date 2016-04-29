using UnityEngine;
using System.Collections;

[System.Serializable]
public class Cell
{
	public enum CellType{OutOfBounds, Corner, BoundLine, Field, InsideBox}
	//public CharacterController character;
	public bool bHighlighted;
	public int id;
	public CellType type;
	public UnitController UnitOccupier{
		get
		{
			return unit;
		}
		set
		{
			unit = value; 

			if(value==null)
				occupied = false;
			else occupied = true;
		}
	}
	public Vector3 Location{get{return location;}}
	//public bool HasBall{get{return IsVectorInCell(Grid_Setup.Instance.BallLocation);}}
	public bool bOccupied{get{return occupied;}}//Physics.CheckSphere(Location, 0.5f, characterLayer);}}
	public int team;
	bool occupied;
	UnitController unit;
	//LayerMask characterLayer = 1<<LayerMask.NameToLayer("Characters");
	Vector3 location;
	Bounds bounds;
	GameObject highlighter= null;

	public Cell()
	{
		id=-1;
	}
	public Cell(int newID, int newType, Vector3 loc, Vector3 size)
	{
		id = newID;
		type = (CellType)newType;
		location = loc;
		bounds = new Bounds(loc,size);
	}
	public Cell(int newID, int newType, int Team, Vector3 loc, Vector3 size)
	{
		id = newID;
		type = (CellType)newType;
		location = loc;
		bounds = new Bounds(loc,size);
		team = Team;
	}
	public void SetHighlighter(GameObject obj)
	{
		highlighter = obj;
		highlighter.transform.position = location+Vector3.up*.02f;
		highlighter.SetActive(false);
	}
	public void DestroyHighlighter()
	{
		if(highlighter!=null)
		GameObject.Destroy(highlighter);
	}

	public bool IsVectorInCell(Vector3 spot)
	{
		return bounds.Contains(spot);
	}
	public Hashtable SaveCell()
	{
		Hashtable ht = new Hashtable();
		ht["location"] = Location; 
		ht["type"] = (int)type; 
		ht["id"] = id; 
		return ht;
	}
	public void Highlight(bool set)
	{
		if(set)
		{
			highlighter.SetActive(true);
		}else highlighter.SetActive(false);
	}
}