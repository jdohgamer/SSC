using UnityEngine;
using System.Collections;

public class ControllerHandler : MessageHandler 
{

	void Awake()
	{
	
	}

	public override void HandleMessage(Message message)
	{
		switch (message.Type) 
		{
			case MessageType.EndTurn:
			{
				
				break;
			}
		}
	}
}
