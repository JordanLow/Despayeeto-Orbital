using UnityEngine;

public class ConsumeHitbox : MonoBehaviour
{
	[SerializeField] GameObject owner;
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		if (other.GetComponent<EnemyAI>().AttemptConsume()) {
			if (other.gameObject.tag == "Boss") {
				other.GetComponent<EnemyAI>().UnlockPower();
			} else {
				owner.GetComponent<PlayerStateMachine>().BasicHeal();
			}
		}
	}
}
