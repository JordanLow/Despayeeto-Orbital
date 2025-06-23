using UnityEngine;

public class RollHitbox : AttackHitbox
{
	private GameObject lastHit;
	
    void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		// Roll potentially interacts with other classes as well, for future refactoring
		if (other.gameObject == lastHit) return;
		lastHit = other.gameObject;
		other.GetComponent<EnemyAI>().takeDamage(attackData.damage * owner.actionStates.Rolling.power, attackData.stun);
		owner.RollCollideHelper(other.gameObject);
	}
}
