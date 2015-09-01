using UnityEngine;
using System.Collections;

public class Tiles : MonoBehaviour {

	private HighlightTileSprite highlighter;
	private Grid_Setup gridSetup;
	private Vector3 currentLoc;
	public Color highlightColor;
	Color normalColor;
	public Material tile;
	public Material ogtile;
	private Cell c;



	public void initializeHighlighter (HighlightTileSprite highlighter) {
		this.highlighter = highlighter;

	}

	public void enableHighlighter () {

		this.highlighter.GetComponent<SpriteRenderer> ().enabled = true;
	
	}

	public void disableHighlighter () {

		this.highlighter.GetComponent<SpriteRenderer> ().enabled = false;

	}

	//public void highlightMove () {
	//	enableHighlighter ();
	//	this.highlighter.GetComponent<SpriteRenderer> ().material.color = PotMove;



	// Use this for initialization
	void Start () {
//		normalColor = renderer.material.color;

		ogtile = this.GetComponent<Renderer>().material;


	}
	
	// Update is called once per frame
	void Update () {
	
		if (Input.GetKeyUp(KeyCode.G))
		{
		GameObject playerPos = GameObject.FindGameObjectWithTag("Player");
		currentLoc = playerPos.transform.position;
		Debug.Log (currentLoc);
		
		}

//		if(c )
//		{
//			this.GetComponent<Renderer>().material.color = Color.red;
//		}


		if ( Input.GetKeyDown (KeyCode.D))
		{
			enableHighlighter();
		
		} else if 
			(Input.GetKeyUp(KeyCode.D))
		{
			disableHighlighter();
		}


		}
	void OnMouseOver() {
		 print = GetComponent<Renderer>().material.GetFloat("_MainTex");
		Debug.Log(print);
		 
		}
	void OnMouseExit() {
		Renderer rend = GetComponent<Renderer>();
		rend.material.shader = Shader.Find("RotateUVS");
		print(rend.material.GetFloat("_Shininess"));
		}
	}

	

