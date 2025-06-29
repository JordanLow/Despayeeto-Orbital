using UnityEngine;

public class ScratchHitbox : EnemyHitbox
{
	protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		other.GetComponent<PlayerStateMachine>().getHit(owner, damage);
		owner.GetComponent<PangolinAI>().SlashReaction(other.GetComponent<PlayerStateMachine>());
	}
}