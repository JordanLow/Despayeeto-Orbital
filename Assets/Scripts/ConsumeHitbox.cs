using UnityEngine;

public class ConsumeHitbox : MonoBehaviour
{
	[SerializeField] GameObject owner;
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		other.GetComponent<EnemyAI>().attemptConsume();
	}
}
