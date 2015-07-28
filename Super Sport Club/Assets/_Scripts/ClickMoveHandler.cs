using UnityEngine;
using System.Collections;

public class ClickMoveHandler : MessageHandler
{
	public float deltaSpeed;
	[SerializeField] protected iTween.EaseType ease;
	protected string easeType;
	//[SerializeField] protected int ;
	[SerializeField] protected float speed;
	public override void HandleMessage(Message message)
	{
		Vector3 target = new Vector3 (message.Vector3Value.x, 0f, message.Vector3Value.z);
		Debug.Log("God damn nigga "+ message.Vector3Value.ToString());
		StopCoroutine ("MoveTo");
		StartCoroutine ("MoveTo",target);
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
