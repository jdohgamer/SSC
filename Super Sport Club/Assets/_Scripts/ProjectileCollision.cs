using UnityEngine;
using System.Collections;

public class ProjectileCollision : MonoBehaviour 
{
	Message hit;
	[SerializeField] string projectile;

	public void OnTriggerEnter(Collider other)
	{
		hit = new Message();
		hit.Type = MessageType.Hit;
		hit.GameObjectValue = other.gameObject;
		hit.StringValue = projectile;
		MessageBus.Instance.SendMessage(hit);
	}
}
