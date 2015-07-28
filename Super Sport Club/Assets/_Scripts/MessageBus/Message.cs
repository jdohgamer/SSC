using UnityEngine;

[System.Serializable]
public enum MessageType
{
	NONE,
	MouseClick,
	PlayerPosition,
	EndTurn,
	Hit
}

public struct Message
{
	public MessageType Type;
	public string StringValue;
	public int IntValue;
	public float FloatValue;
	public Vector3 Vector3Value;
	public GameObject GameObjectValue;
}