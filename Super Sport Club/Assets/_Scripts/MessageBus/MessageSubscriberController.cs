using UnityEngine;
using System.Collections;

public class MessageSubscriberController : MonoBehaviour 
{
	public MessageType[] MessageTypes;
	public MessageHandler Handler;
	MessageSubscriber subscriber;

	void Start()
	{
		subscriber = new MessageSubscriber ();
		subscriber.MessageTypes = MessageTypes;
		subscriber.Handler = Handler;

		MessageBus.Instance.AddSubscriber (subscriber);
	}
	void OnDestroy()
	{
		MessageBus.Instance.RemoveSubscriber(subscriber);
	}
}
