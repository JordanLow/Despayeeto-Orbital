using UnityEngine;

public class ImpaleTrail : MonoBehaviour
{
	
	private float startTime;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - startTime >= 3f) {
			Destroy(gameObject);
		}
    }
}
