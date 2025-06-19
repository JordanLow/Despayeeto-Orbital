using UnityEngine;

public class SpineHitbox : MonoBehaviour
{
	[SerializeField] private float damage;
	[SerializeField] private float speed;
	[SerializeField] private float stunDuration;

    // Update is called once per frame
    void Update() {
        transform.position += transform.up * speed * Time.deltaTime;
    }
	
	void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;
        other.GetComponent<EnemyAI>().takeDamage(damage, stunDuration);
		Destroy(gameObject);
    }
}
