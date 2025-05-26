using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
	[SerializeField] private float speed;
	
    private Rigidbody rb;
	private NavMeshPath path;
	private int pathIdx;
	private Vector3 velocity;
	
	private enum State {
		Wandering,
		Chasing
	}
	
	private State state;
	private float curLimit = 0f;
	private float rLimit = 0.4f;
	
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		velocity = Vector3.zero;
		state = State.Wandering;
		
		GetAimlessDestination();
	}

    void FixedUpdate()
	{
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f, LayerMask.GetMask("Ground"));
		if (state == State.Wandering) {
			Collider[] hits = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Player"));
			if (hits.Length > 0)
			{
				Debug.Log("Player Detected");
				Transform nearestPlayer = hits[0].transform;
				state = State.Chasing;
				curLimit = 0f;
				PathfindToPlayer(nearestPlayer);
			}
		} else if (state == State.Chasing) {
			Collider[] hits = Physics.OverlapSphere(transform.position, 3f, LayerMask.GetMask("Player"));
			if (hits.Length <= 0)
			{
				Debug.Log("Player lost");
				state = State.Wandering;
				GetAimlessDestination();
			} else if (curLimit >= rLimit) {
				PathfindToPlayer(hits[0].transform);
				curLimit = 0f;
				Debug.Log("Pathing to " + hits[0].transform.position);
			} else {
				curLimit += Time.fixedDeltaTime;
			}
		}
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
			rb.MoveRotation(targetRotation);
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
				return;
			}
		}
	}
	
	private void PathfindToPlayer(Transform player) {
		for (int i = 0; i < 10; i++) {
			if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path);
				Debug.Log(player.transform.position);
				pathIdx = 0;
				return;
			}
		}
	}
}
