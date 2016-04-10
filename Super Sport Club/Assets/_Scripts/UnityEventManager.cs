using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class IntEvent: UnityEvent<int>
{
}
public class UnityEventManager : MonoBehaviour 
{

	private Dictionary <string, UnityEvent> eventDictionary;
	private Dictionary <string, IntEvent> IntEventDictionary;

	private static UnityEventManager eventManager;

	public static UnityEventManager instance
	{
		get
		{
			if (!eventManager)
			{
				eventManager = FindObjectOfType (typeof (UnityEventManager)) as UnityEventManager;

				if (!eventManager)
				{
					Debug.LogError ("There needs to be one active EventManger script on a GameObject in your scene.");
				}
				else
				{
					eventManager.Init (); 
				}
			}

			return eventManager;
		}
	}

	void Init ()
	{
		if (eventDictionary == null)
		{
			eventDictionary = new Dictionary<string, UnityEvent>();
			IntEventDictionary = new Dictionary<string, IntEvent>();
		}
	}

	public static void StartListening (string eventName, UnityAction listener)
	{
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.AddListener (listener);
		} 
		else
		{
			thisEvent = new UnityEvent ();
			thisEvent.AddListener (listener);
			instance.eventDictionary.Add (eventName, thisEvent);
		}
	}
	public static void StartListeningInt (string eventName, UnityAction<int> listener)
	{
		IntEvent thisIntEvent = null;
		if (instance.IntEventDictionary.TryGetValue (eventName, out thisIntEvent))
		{
			thisIntEvent.AddListener (listener);
		} 
		else
		{
			thisIntEvent = new IntEvent ();
			thisIntEvent.AddListener (listener);
			instance.IntEventDictionary.Add (eventName, thisIntEvent);
		}
	}

	public static void StopListening (string eventName, UnityAction listener)
	{
		if (eventManager == null) return;
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.RemoveListener (listener);
		}
	}
	public static void StopListeningInt (string eventName, UnityAction<int> listener)
	{
		if (eventManager == null) return;
		IntEvent thisEvent = null;
		if (instance.IntEventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.RemoveListener (listener);
		}
	}

	public static void TriggerEvent (string eventName)
	{
		UnityEvent thisEvent = null;
		if (instance.eventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.Invoke ();
		}
	}
	public static void TriggerEventInt (string eventName, int i)
	{
		IntEvent thisEvent = null;
		if (instance.IntEventDictionary.TryGetValue (eventName, out thisEvent))
		{
			thisEvent.Invoke (i);
		}
	}

}