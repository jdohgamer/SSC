using UnityEngine;
using System.Collections;

public abstract class MessageHandler : MonoBehaviour
{
	public abstract void HandleMessage( Message message );
}