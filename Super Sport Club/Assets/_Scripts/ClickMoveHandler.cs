using UnityEngine;
using System.Collections;

public class ClickMoveHandler : MessageHandler
{
	//public float deltaSpeed;
	[SerializeField] protected iTween.EaseType ease;
	protected string easeType;
	public Vector3 target;
	[SerializeField] protected float speed;
	public override void HandleMessage(Message message)
	{
		switch (message.Type) 
		{
			case MessageType.MouseClick:
			{
				target = new Vector3 (message.Vector3Value.x, 0f, message.Vector3Value.z);
				Debug.Log("God damn nigga "+ message.Vector3Value.ToString());
				break;
			}
			case MessageType.EndTurn:
			{
				StopCoroutine ("MoveTo");
				StartCoroutine ("MoveTo",target);
				break;
			}
		}
	}
	IEnumerator MoveTo(Vector3 target)
	{
		easeType = ease.ToString();
		while (Vector3.Distance(transform.position,target)>.1f) 
		{
			iTween.MoveTo(gameObject, iTween.Hash("position", target, "easeType", easeType, "loopType", "none", "speed", speed));
			//transform.position = Vector3.MoveTowards (transform.position, target, deltaSpeed);
			yield return null;
		}
	}
}
