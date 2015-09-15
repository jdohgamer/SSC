using UnityEngine;
using System.Collections;

[System.Serializable]
public class Cell
{
	public FSM_Character character;
	//public GameObject boardObj;
	public bool bHighlighted, hasBall, bOccupied;
	public int id;
	//public CellMono cm;
	public CellType type;
	public enum CellType{OutOfBounds, Corner, BoundLine, Field, InsideBox}
	public Vector3 Location{get{return location;}}
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
	public void SetHighlighter(GameObject obj)
	{
		highlighter = obj;
		if(location != null)
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
		ht["character"] = character.id; 
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
//public class CellMono: MonoBehaviour
//{
//	public Material mat;
//
//	void Awake()
//	{
//
//		mat = GetComponent<MeshRenderer>().material;
//
//	}
//
////	void OnMouseEnter()
////	{
////		//Highlight(true);
////		Debug.Log(transform.GetSiblingIndex());
////	}
////	void OnMouseExit()
////	{
////		//Highlight(false);
////		Debug.Log(transform.GetSiblingIndex());
////	}
//	public void Highlight(bool set)
//	{
//		if(set)
//		{
//			mat.SetColor ("_Color",Color.cyan);
//			mat.SetFloat ("_Alpha",0.2f);
//		}
//		else{
//			mat.SetColor("_Color",Color.black);
//			mat.SetFloat ("_Alpha",1f);
//		}
//	}
//
////	public void RotateMat(float degrees)
////	{
////		if(mat.GetFloat("_RotationDegree")!= null)
////		{
////			mat.SetFloat("_RotationDegree", degrees* Mathf.Deg2Rad);
////		}
////	}
//}