using UnityEngine;
using System.Collections;

[System.Serializable]
public class Cell
{
	public FSM_Character character;
	public GameObject boardObj;
	public bool bHighlighted, hasBall, bOccupied;
	public int x,z,id;
	public CellMono cm;
	public CellType type;
	public enum CellType{OutOfBounds, Corner, BoundLine, Field, InsideBox}

	public Cell(int newID)
	{
		id = newID;
	}
	public Cell(int newID, int newType)
	{
		id = newID;
		type = (CellType)newType;
	}
	public Cell(int newID, int newType, int X, int Z)
	{
		id = newID;
		type = (CellType)newType;
		x = X;
		z = Z;
	}

	public Hashtable SaveCell()
	{
		Hashtable ht = new Hashtable();
		ht["type"] = (int)type; 
		return ht;
	}
}
public class CellMono: MonoBehaviour
{
	Material mat;

	void Awake()
	{
		mat = GetComponent<Renderer>().material;
	}

	public void RotateMat(float degrees)
	{
		if(mat.GetFloat("_RotationDegree")!= null)
		{
			mat.SetFloat("_RotationDegree", degrees* Mathf.Deg2Rad);
		}
	}
}