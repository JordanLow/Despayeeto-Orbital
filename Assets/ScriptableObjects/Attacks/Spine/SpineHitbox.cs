using UnityEngine;

public class SpineHitbox : AttackHitbox
{
	[SerializeField] private float speed;

    // Update is called once per frame
    void Update() {
        transform.position += transform.up * speed * Time.deltaTime;
    }
	
	void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
        other.GetComponent<EnemyAI>().takeDamage(attackData.damage, attackData.stun, owner);
		Destroy(gameObject);
    }
}
