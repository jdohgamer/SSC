using UnityEngine;
using System.Collections;

public class ControllerHandler : MessageHandler 
{
	MouseClick mouse;
	void Awake()
	{
		mouse = GetComponent<MouseClick> ();
	}

	public override void HandleMessage(Message message)
	{
		switch (message.Type) 
		{
			case MessageType.EndTurn:
			{
				mouse.EndTurn();
				break;
			}
		}
	}
}
