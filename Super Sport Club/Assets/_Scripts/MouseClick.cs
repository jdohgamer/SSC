using UnityEngine;
using System.Collections;

public class MouseClick : MonoBehaviour {

	Message message;
	public Vector3 p;
	void Start () 
	{
		message = new Message ();
	}
	public void OnMouseUp() 
	{
		p = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		message.Type = MessageType.MouseClick;
		message.Vector3Value = p;
		MessageBus.Instance.SendMessage (message);

	}
}
