using UnityEngine;

public class ImpaleSeeker : MonoBehaviour
{
	
	[SerializeField] GameObject trail;
	[SerializeField] GameObject spike;
	[SerializeField] float trailIntervals;
	public GameObject target;
	private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
		timer += Time.deltaTime;
		Vector3 enemyDir = target.transform.position - transform.position;
		enemyDir.y = 0f;
		if (timer >= trailIntervals) {
			if (enemyDir.magnitude <= 0.4f) {
				transform.position += enemyDir;
				Instantiate(trail, transform.position, Quaternion.identity);
				Debug.Log(transform.position);
				Instantiate(spike, transform.position, Quaternion.identity);
				Destroy(gameObject);
			} else {
				timer = 0f;
				transform.position = transform.position + Time.deltaTime * enemyDir.normalized * 2f;
				Instantiate(trail, transform.position, Quaternion.identity);
			}
		} else {
			transform.position = transform.position + Time.deltaTime * enemyDir.normalized * 2f;
		}
    }
}
