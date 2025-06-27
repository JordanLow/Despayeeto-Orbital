using UnityEngine;

public class LeapHitbox : EnemyHitbox
{
    protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		other.GetComponent<PlayerStateMachine>().getHit(owner, damage);
		owner.GetComponent<FrogAI>().LeapCollideHelper();
	}
}
