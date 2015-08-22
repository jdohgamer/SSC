using UnityEngine;
using System.Collections;

public class ClickMoveHandler : MessageHandler
{
	//public float deltaSpeed;
	FSM_Character player;

	void Awake()
	{
		player = GetComponent<FSM_Character>();
	}

	public override void HandleMessage(Message message)
	{
		switch (message.Type) 
		{
			case MessageType.MouseClick:
			{
				if(MouseClick.CurrentSelectedObj == this.gameObject)
				{
					Vector3 target = new Vector3 (message.Vector3Value.x, 0.2f, message.Vector3Value.z);
					Debug.Log("God damn nigga "+ message.Vector3Value.ToString());
					//player.MoveTarget = target;
				}
//				else if (message.GameObjectValue == this.gameObject)
//				{
//					player.bSelected = true;
//				}
			break;
			}
			case MessageType.EndTurn:
			{
				player.EndTurn();
				break;
			}
		}
	}

}
