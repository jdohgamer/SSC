using UnityEngine;
using System.Collections;

public class MouseClick : MonoBehaviour {

	Message message;
	public static bool bSelection;
	public static GameObject CurrentSelectedObj;
	[SerializeField] LayerMask mask;
	void Start () 
	{
		CurrentSelectedObj = new GameObject ();
		message = new Message ();
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
						bSelection = true;
						CurrentSelectedObj = hit.transform.gameObject;
						Debug.Log ("Dick");
						break;
					}
					case "Field":
					{
						if(bSelection)
						{
							Transform objectHit = hit.transform;
							message.Vector3Value = objectHit.position;
							Debug.Log ("Click");
							message.Type = MessageType.MouseClick;
							MessageBus.Instance.SendMessage (message);
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
