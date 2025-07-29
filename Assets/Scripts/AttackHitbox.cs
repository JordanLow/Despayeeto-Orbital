using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
	[SerializeField] protected PlayerAttack attackData;
	[SerializeField] protected PlayerStateMachine owner;
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		other.GetComponent<EnemyAI>().takeDamage(attackData.damage, attackData.stun, owner);
	}
	
	
}
