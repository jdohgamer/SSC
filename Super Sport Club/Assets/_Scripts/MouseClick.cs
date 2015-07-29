using UnityEngine;
using System.Collections;

public class MouseClick : MonoBehaviour {

	Message message;
	public Vector3 p;
	public static bool BSelection;
	[SerializeField] LayerMask mask;
	void Start () 
	{
		message = new Message ();
	}
	public void Update() 
	{
		if (Input.GetMouseButtonUp (0)) 
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		
			if (Physics.Raycast (ray, out hit, 100f, mask)) 
			{
				Transform objectHit = hit.transform;
				message.Type = MessageType.MouseClick;
				message.Vector3Value = objectHit.position;
				message.Vector3Value += new Vector3 (0,1,0);
				MessageBus.Instance.SendMessage (message);
				Debug.Log ("Click");
			}
			//p = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		}
	}
}
