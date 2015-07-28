using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageBus
{

	Dictionary<MessageType, List<MessageSubscriber>> subscriberLists = new Dictionary<MessageType, List<MessageSubscriber>>();

	public void AddSubscriber( MessageSubscriber subscriber )
	{
		MessageType[] messageTypes = subscriber.MessageTypes;
		for (int i = 0; i < messageTypes.Length; i++)
			AddSubscriberToMessage (messageTypes[i], subscriber);
	}

	void AddSubscriberToMessage ( MessageType messageType, MessageSubscriber subscriber)
	{
		if (!subscriberLists.ContainsKey (messageType))
			subscriberLists [messageType] = new List<MessageSubscriber> ();

		subscriberLists [messageType].Add (subscriber);
	}

	public void RemoveSubscriber( MessageSubscriber subscriber )
	{
		MessageType[] messageTypes = subscriber.MessageTypes;
		for (int i = 0; i < messageTypes.Length; i++)
			RemoveSubscriberToMessage (messageTypes[i], subscriber);
	}
	void RemoveSubscriberToMessage ( MessageType messageType, MessageSubscriber subscriber)
	{
		if (subscriberLists.ContainsKey (messageType))
		subscriberLists [messageType].Remove (subscriber);
	}

	public void SendMessage (Message message)
	{
		if (!subscriberLists.ContainsKey (message.Type))
			return;

		List<MessageSubscriber> subscriberList = 
			subscriberLists [message.Type];

		for (int i = 0; i < subscriberList.Count; i++)
			SendMessageToSubscriber (message, subscriberList [i]);
	}

	void SendMessageToSubscriber(Message message, MessageSubscriber subscriber)
	{
		subscriber.Handler.HandleMessage (message);
	}

	/* Singleton */
	static MessageBus instance;

	public static MessageBus Instance
	{
		get
		{
			if(instance == null)
				instance = new MessageBus();

			return instance;
		}
	}

	private MessageBus() {}
}
