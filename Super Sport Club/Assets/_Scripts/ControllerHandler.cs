using UnityEngine;
using System.Collections;

public class ControllerHandler : MessageHandler 
{
	GameController gc;
	void Awake()
	{
		gc = GetComponent<GameController> ();
	}

	public override void HandleMessage(Message message)
	{
		switch (message.Type) 
		{
			case MessageType.EndTurn:
			{
				gc.EndTurn();
				break;
			}
		}
	}
}
