using UnityEngine;

public class RollHitbox : AttackHitbox
{	
    void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		other.GetComponent<EnemyAI>().takeDamage(attackData.damage * owner.GetComponent<PlayerStateMachine>().actionStates.Rolling.power);
	}
}
