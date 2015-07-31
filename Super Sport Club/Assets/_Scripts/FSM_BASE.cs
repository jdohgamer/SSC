using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class FSM_Base : MonoBehaviour {

	public Action DoUpdate = DoNothing;
	public Action DoLateUpdate = DoNothing;
	public Action DoFixedUpdate = DoNothing;
	public Action<Collider> DoOnTriggerEnter = DoNothingCollider;
	public Action<Collider> DoOnTriggerStay = DoNothingCollider;
	public Action<Collider> DoOnTriggerExit = DoNothingCollider;
	public Action<Collision> DoOnCollisionEnter = DoNothingCollision;
	public Action<Collision> DoOnCollisionStay = DoNothingCollision;
	public Action<Collision> DoOnCollisionExit = DoNothingCollision;
	public Action DoOnMouseEnter = DoNothing;
	public Action DoOnMouseUp = DoNothing;
	public Action DoOnMouseDown = DoNothing;
	public Action DoOnMouseOver = DoNothing;
	public Action DoOnMouseExit = DoNothing;
	public Action DoOnMouseDrag = DoNothing;
	public Action DoOnGUI = DoNothing;
	public Func<IEnumerator> EnterState = DoNothingCoroutine;
	public Func<IEnumerator> ExitState = DoNothingCoroutine;
	public Action DoOnTargetReached = DoNothing;

	private Enum _currentState;
	
	public Enum currentState
	{
		get
		{
			return _currentState;
		}
		set
		{
			_currentState = value;
			ConfigureCurrentState();			
		}
	}

	static void DoNothing()
	{}
	static void DoNothingCollider(Collider other)
	{}
	static void DoNothingCollision(Collision other)
	{}
	static IEnumerator DoNothingCoroutine()
	{
		yield break;
	}

	void ConfigureCurrentState()
	{
		//If we have an exit state, then start it as a 
		//coroutine
		if(ExitState != null)
		{
			StartCoroutine(ExitState());
		}
		//Now we need to configure all of the methods
		DoUpdate = ConfigureDelegate<Action>("Update", DoNothing);
		DoOnGUI = ConfigureDelegate<Action>("OnGUI", DoNothing);
		DoLateUpdate = ConfigureDelegate<Action>("LateUpdate", DoNothing);
		DoFixedUpdate = ConfigureDelegate<Action>("FixedUpdate", DoNothing);
		DoOnMouseUp = ConfigureDelegate<Action>("OnMouseUp", DoNothing);
		DoOnMouseDown = ConfigureDelegate<Action>("OnMouseDown", DoNothing);
		DoOnMouseEnter = ConfigureDelegate<Action>("OnMouseEnter", DoNothing);
		DoOnMouseExit = ConfigureDelegate<Action>("OnMouseExit", DoNothing);
		DoOnMouseDrag = ConfigureDelegate<Action>("OnMouseDrag", DoNothing);
		DoOnMouseOver = ConfigureDelegate<Action>("OnMouseOver", DoNothing);
		DoOnTriggerEnter = ConfigureDelegate<Action<Collider>>("OnTriggerEnter", DoNothingCollider);
		DoOnTriggerExit = ConfigureDelegate<Action<Collider>>("OnTriggerExit", DoNothingCollider);
		DoOnTriggerStay = ConfigureDelegate<Action<Collider>>("OnTriggerStay", DoNothingCollider);
		DoOnCollisionEnter = ConfigureDelegate<Action<Collision>>("OnCollisionEnter", DoNothingCollision);
		DoOnCollisionExit = ConfigureDelegate<Action<Collision>>("OnCollisionExit", DoNothingCollision);
		DoOnCollisionStay = ConfigureDelegate<Action<Collision>>("OnCollisionStay", DoNothingCollision);
		EnterState = ConfigureDelegate<Func<IEnumerator>>("EnterState", DoNothingCoroutine);
		ExitState = ConfigureDelegate<Func<IEnumerator>>("ExitState", DoNothingCoroutine);
		DoOnTargetReached = ConfigureDelegate<Action>("OnTargetReached", DoNothing);
		//Optimisation, turn off GUI if we don't
		//have an OnGUI method
		//EnableGUI();
		//Start the current state
		StartCoroutine(EnterState());
		
	}

	//Define a generic method that returns a delegate
	//Note the where clause - we need to ensure that the
	//type passed in is a class and not a value type or our
	//cast (As T) will not work
	T ConfigureDelegate<T>(string methodRoot, T Default) where T : class
	{
		//Find a method called CURRENTSTATE_METHODROOT
		//The method can be either public or private
		var mtd = GetType().GetMethod(_currentState.ToString() + "_" + methodRoot, System.Reflection.BindingFlags.Instance 
		                              | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod);
		//If we found a method
		if(mtd != null)
		{
			//Create a delegate of the type that this
			//generic instance needs and cast it                    
			return Delegate.CreateDelegate(typeof(T), this, mtd) as T;
		}
		else
		{
			//If we didn't find a method return the default
			return Default;
		}
		
	}

	void Update () 
	{
		DoUpdate ();
	}
	void FixedUpdate () 
	{
		DoFixedUpdate ();
	}
	void LateUpdate () 
	{
		DoLateUpdate ();
	}
	void OnTargetReached()
	{
		DoOnTargetReached ();
	}
	void OnTriggerEnter(Collider other)
	{
		DoOnTriggerEnter (other);
	}
	void OnTriggerStay(Collider other)
	{
		DoOnTriggerStay (other);
	}
	void OnTriggerExit(Collider other)
	{
		DoOnTriggerExit (other);
	}
	void OnCollisionEnter(Collision other)
	{
		DoOnCollisionEnter (other);
	}
	void OnCollisionStay(Collision other)
	{
		DoOnCollisionStay (other);
	}
	void OnCollisionExit(Collision other)
	{
		DoOnCollisionExit(other);
	}

}

