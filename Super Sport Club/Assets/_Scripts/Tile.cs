using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	private HighlightTileSprite highlighter;
	private Grid_Setup gridSetup;
	private Vector3 currentLoc;
	public Color highlightColor;
	Color normalColor;
	public Material tile;
	public Material ogtile;



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


		GameObject playerPos = GameObject.FindGameObjectWithTag("Player");
		currentLoc = playerPos.transform.position;
		Debug.Log (currentLoc);

	}
	
	// Update is called once per frame
	void Update () {
	
	



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
		GetComponent<Renderer>().material.color = Color.red;
		}
	void OnMouseExit() {
			GetComponent<Renderer>().material = ogtile;
		}
	}

	

