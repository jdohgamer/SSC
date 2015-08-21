using UnityEngine;
using System.Collections;

[System.Serializable]
public class Cell
{
	public FSM_Character character;
	public GameObject boardObj;
	public bool bHighlighted, hasBall, bOccupied;
	public int x,z,id;
	public Vector3 location;
	public CellMono cm;
	public CellType type;
	public enum CellType{OutOfBounds, Corner, BoundLine, Field, InsideBox}

	public Cell()
	{
		id=-1;
	}
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
	public Vector3 GetLocation()
	{
		return boardObj.transform.position;
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
	public Material mat;

	void Awake()
	{
		mat = GetComponent<MeshRenderer>().material;

	}

	void OnMouseEnter()
	{
		//Highlight(true);
		Debug.Log(transform.GetSiblingIndex());
	}
	void OnMouseExit()
	{
		//Highlight(false);
		Debug.Log(transform.GetSiblingIndex());
	}
	public void Highlight(bool set)
	{
		if(set)
		{
			mat.SetColor ("_Color",Color.cyan);
			mat.SetFloat ("_Alpha",0.2f);
		}
		else{
			mat.SetColor("_Color",Color.black);
			mat.SetFloat ("_Alpha",1f);
		}
	}

//	public void RotateMat(float degrees)
//	{
//		if(mat.GetFloat("_RotationDegree")!= null)
//		{
//			mat.SetFloat("_RotationDegree", degrees* Mathf.Deg2Rad);
//		}
//	}
}