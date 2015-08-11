using UnityEngine;
using System.Collections;
[System.Serializable]
public class Cell: MonoBehaviour
{
	public FSM_Character character;
	public bool hasBall;
	//public CellMono cm;
	public bool bHighlighted;
	public Material mat;
}