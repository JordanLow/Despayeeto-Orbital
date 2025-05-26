using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
	[SerializeField] private float speed;
	
    private Rigidbody rb;
	private NavMeshPath path;
	private int pathIdx;
	private Vector3 velocity;
	
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		velocity = Vector3.zero;
		
		GetAimlessDestination();
	}

    void FixedUpdate()
	{
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f, LayerMask.GetMask("Ground"));
		if (path != null && pathIdx < path.corners.Length) {
			Vector3 dir = (path.corners[pathIdx] - transform.position);
			dir.y = 0f;
			if (dir.magnitude < 0.2f) {
				pathIdx++;
			} else {
				Vector3 move = dir.normalized * speed;
				velocity.x = move.x;
				velocity.z = move.z;
			}
			Quaternion targetRotation = Quaternion.LookRotation(dir);
			rb.rotation = targetRotation;
		} else {
			velocity.x = 0;
			velocity.z = 0;
			GetAimlessDestination();
		}
		Vector3 dest = rb.position + velocity * Time.fixedDeltaTime;
		if (!isGrounded && velocity.y > Physics.gravity.y) velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
		if (isGrounded) {
			dest.y = hit.point.y + 0.01f;
            rb.MovePosition(dest);
			velocity.y = 0;
		} else {
			rb.MovePosition(dest);
		}
	}
	
	private void GetAimlessDestination() {
		for (int i = 0; i < 10; i++) {
			Vector3 randPoint = transform.position + Random.insideUnitSphere * 5f;
			if (NavMesh.SamplePosition(randPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path);
				pathIdx = 0;
			}
		}
	}
}
