using UnityEngine;
using System.Collections;

public class TweenBy : MonoBehaviour 
{
	[SerializeField] protected iTween.EaseType ease;
	protected string easeType;
	//[SerializeField] protected int ;
	[SerializeField] protected float delay, killTime, dist, speed;
	protected virtual void Start () 
	{
		easeType = ease.ToString();
		iTween.MoveBy(gameObject, iTween.Hash("z", dist, "easeType", easeType, "loopType", "none", "speed", speed, "delay", delay));
		Destroy (gameObject,killTime);
	}

	//IEnumerator 
}
