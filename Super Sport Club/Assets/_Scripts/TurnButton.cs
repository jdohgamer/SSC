using UnityEngine;
using System.Collections;

public class TurnButton : MonoBehaviour {

	Message message;
	public void EndTurn()
	{
		message = new Message ();
		message.Type = MessageType.EndTurn;
		MessageBus.Instance.SendMessage (message);
	}
}
