using UnityEngine;

public class ConsumeHitbox : MonoBehaviour
{
	[SerializeField] GameObject owner;
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		if (other.GetComponent<EnemyAI>().attemptConsume()) {
			// For now just Basic Consume logic, evolution refactor later
			owner.GetComponent<PlayerStateMachine>().BasicHeal();
		}
	}
}
