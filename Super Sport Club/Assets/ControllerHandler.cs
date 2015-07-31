using UnityEngine;
using System.Collections;

public class ControllerHandler : MessageHandler 
{

	public override void HandleMessage(Message message)
	{
		switch (message.Type) 
		{
			case MessageType.EndTurn:
			{
				MouseClick.CurrentSelectedObj = null;
				MouseClick.bSelection = false;
				break;
			}
		}
	}
}
