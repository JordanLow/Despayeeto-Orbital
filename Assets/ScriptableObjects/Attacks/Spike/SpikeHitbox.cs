using UnityEngine;

public class SpikeHitbox : AttackHitbox
{
	[SerializeField] private float spikeDuration;
	private float startTime;
	private bool retracting = false;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!retracting && Time.time - startTime >= spikeDuration) {
			GetComponent<Animator>().Play("Retract");
			retracting = true;
		}
    }
	
	void Cleanup() {
		Destroy(gameObject);
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
		// Roll potentially interacts with other classes as well, for future refactoring
		other.GetComponent<EnemyAI>().takeDamage(attackData.damage, spikeDuration + 0.1f, owner);
	}
}
