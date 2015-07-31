using UnityEngine;

[System.Serializable]
public enum MessageType
{
	NONE,
	MouseClick,
	BallPosition,
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

	public void Clear()
	{
		this.StringValue = "";
		this.IntValue = 0;
		this.FloatValue = 0f;
		this.Vector3Value = Vector3.zero;
		this.GameObjectValue = null;
	}
}