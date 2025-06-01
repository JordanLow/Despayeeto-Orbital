using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
	[SerializeField] PlayerAttack attackData;
	[SerializeField] GameObject owner;
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		other.GetComponent<EnemyAI>().takeDamage(attackData.damage);
	}
	
	
}
