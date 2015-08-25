using UnityEngine;
using System.Collections;

public class HitHandler : MessageHandler
{
	public override void HandleMessage(Message message)
	{
		if(this.gameObject == message.GameObjectValue)
		{
			Debug.Log("God damn nigga");
		}
	}
}
