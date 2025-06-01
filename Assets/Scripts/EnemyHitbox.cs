using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
	[SerializeField] GameObject owner;
	[SerializeField] float damage;
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		other.GetComponent<PlayerStateMachine>().getHit(owner, damage);
	}
}
