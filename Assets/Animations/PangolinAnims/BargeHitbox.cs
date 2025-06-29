using UnityEngine;

public class BargeHitbox : EnemyHitbox
{
    
	protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		other.GetComponent<PlayerStateMachine>().getHit(owner, damage);
		owner.GetComponent<PangolinAI>().BargeReaction(other.GetComponent<PlayerStateMachine>());
	}
}
