using UnityEngine;

public class BladeHitbox : EnemyHitbox
{
    [SerializeField] Collider hitbox;
	
	protected override void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
		bool shell = other.GetComponent<PlayerStateMachine>().shelled;
		if (!shell) {
			other.GetComponent<PlayerStateMachine>().getHit(owner, damage);
		}
		owner.GetComponent<SpinningBladeScript>().HitboxHelper(shell);
	}
	
	public void EnableHitbox() {
		// Debug.Log("Enabling Blade");
		hitbox.enabled = true;
	}
	
	public void DisableHitbox() {
		// Debug.Log("Disabling Blade");
		hitbox.enabled = false;
	}
}
