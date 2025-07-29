using UnityEngine;
using System.Collections.Generic;

public class EmulsionZone : MonoBehaviour
{
    private HashSet<EnemyAI> hits;
	public PlayerStateMachine owner;
	private float timer = 5f;
	
	void Awake() {
		hits = new HashSet<EnemyAI>();
	}
	
	void Update() {
		timer -= Time.deltaTime;
		if (timer <= 0f) {
			Collider col = GetComponent<Collider>();
			Collider[] overlaps = Physics.OverlapSphere(transform.position, 1.25f);
			foreach (Collider other in overlaps)
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
					other.GetComponent<PlayerStateMachine>().ExitEmulsion();
				}
            }
			Destroy(transform.parent.gameObject);
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
			if (hits.Contains(other.GetComponent<EnemyAI>())) {
				return;
			}
			hits.Add(other.GetComponent<EnemyAI>());
			other.GetComponent<EnemyAI>().takeDamage(15f, 0f, owner);
		} else if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
			other.GetComponent<PlayerStateMachine>().EnterEmulsion();
		}
	}
	
	void OnTriggerExit(Collider other) {
		if (other.gameObject.layer == LayerMask.NameToLayer("Player")) {
			other.GetComponent<PlayerStateMachine>().ExitEmulsion();
		}
	}
}
