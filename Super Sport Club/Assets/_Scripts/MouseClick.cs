using UnityEngine;
using System.Collections;

public class MouseClick : MonoBehaviour {

	Message message;
	public static bool bSelection;
	public static GameObject CurrentSelectedObj;
	FSM_Character character;
	public GameObject destPin;
	GameObject newPin, oldPin;
	MeshRenderer currentMesh;
	Grid_Setup grid;
	CustomGameClient GameClientInstance;
	[SerializeField] LayerMask mask;
	void Start () 
	{
		CurrentSelectedObj = new GameObject ();
		newPin = new GameObject ();
		currentMesh = new MeshRenderer ();
		message = new Message ();
	}
	public void EndTurn()
	{
		if(currentMesh !=null)
			currentMesh.material.color = Color.white;
		currentMesh = null;
		CurrentSelectedObj = null;
		bSelection = false;
	}
	public void FixedUpdate() 
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			message.Clear();
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
			if (Physics.Raycast (ray, out hit, 100f, mask)) 
			{
				switch(hit.transform.tag)
				{
					case "Player":
					{
						
						if(CurrentSelectedObj != hit.transform.gameObject)
						{
							bSelection = true;
							if(currentMesh !=null)
							{
								currentMesh.material.color = Color.white;
							}
							oldPin = newPin;
							newPin = null;
							CurrentSelectedObj = hit.transform.gameObject;
							currentMesh = CurrentSelectedObj.GetComponent<MeshRenderer>();
							currentMesh.material.color = Color.cyan;
							character = CurrentSelectedObj.GetComponent<FSM_Character>();
							
							Debug.Log ("Dick");
						}
						break;
					}
					case "Field":
					{
						if(bSelection)
						{

							Transform objectHit = hit.transform;
							int index  = hit.transform.GetSiblingIndex();
							Cell cell =  grid.cells[index];
							
							GameClientInstance.SetPlayerAction(PlayerAction.Actions.Move, character, cell);
							
							message.Vector3Value = objectHit.position ;
							Debug.Log ("Click");
							message.Type = MessageType.MouseClick;
							MessageBus.Instance.SendMessage (message);
							if(newPin != null)
							{
								Destroy(newPin);
							}
							newPin = Instantiate(destPin,message.Vector3Value,Quaternion.identity) as GameObject;

						}
						break;
					}
				}
			}
			//p = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		}
		if (Input.GetMouseButtonUp (1)) 
		{
			bSelection = false;
			CurrentSelectedObj = null;
		}
	}
}
