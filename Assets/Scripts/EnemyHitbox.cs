using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
	[SerializeField] protected GameObject owner;
	[SerializeField] protected float damage;
	
	protected virtual void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		other.GetComponent<PlayerStateMachine>().getHit(owner, damage);
	}
}
