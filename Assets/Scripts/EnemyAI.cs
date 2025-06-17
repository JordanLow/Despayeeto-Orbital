using UnityEngine;
using UnityEngine.AI;


// Refactor into extensible for different enemy types in the future
public class EnemyAI : MonoBehaviour
{
	[SerializeField] private float speed;
	
    protected Rigidbody rb;
	private NavMeshPath path;
	private int pathIdx;
	private Vector3 velocity;
	private Animator animator;
	
	[SerializeField] private float HP = 20f;
	[SerializeField] private float Devourable = 10f;
	[SerializeField] public float heightOffset = 0.45f;
	
	private enum State {
		Wandering,
		Chasing,
		Attacking,
		Bounded,
		Stunned,
		Dead
	}
	
	private State state;
	private float curLimit = 0f;
	private float rLimit = 0.4f;
	private float timeDied = 0f;
	public float stunDuration = 0f;
	
	void Start()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		velocity = Vector3.zero;
		state = State.Wandering;
		
		GetAimlessDestination();
	}

    protected virtual void FixedUpdate()
	{
		if (HP <= 0f) {
			state = State.Dead;
			animator.Play("Deflate");
			rb.detectCollisions = false;
		}
		if (state == State.Dead) {
			timeDied += Time.fixedDeltaTime;
			if (timeDied > 2f) Destroy(gameObject);
			return;
		}
		if (stunDuration > 0f && state != State.Stunned) {
			animator.Play("EmptyAnimanim"); // Placeholder for actual stun animation
			state = State.Stunned;
			return;
		}
		if (state == State.Stunned) {
			stunDuration -= Time.fixedDeltaTime;
			if (stunDuration <= 0f) {
				stunDuration = 0f;
				state = State.Wandering;
			} else {
				return;
			}
		}
		if (state == State.Bounded || state == State.Attacking) {
			return;
		}
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f, LayerMask.GetMask("Ground"));
		if (state == State.Wandering) {
			Collider[] hits = Physics.OverlapSphere(transform.position, 4f, LayerMask.GetMask("Player"));
			if (hits.Length > 0) {
				//Debug.Log("Player Detected");
				Transform nearestPlayer = hits[0].transform;
				state = State.Chasing;
				curLimit = 0f;
				PathfindToPlayer(nearestPlayer);
			}
		} else if (state == State.Chasing) {
			Collider[] hits = Physics.OverlapSphere(transform.position, 4f, LayerMask.GetMask("Player"));
			if (hits.Length <= 0) {
				Debug.Log("Player lost");
				state = State.Wandering;
				GetAimlessDestination();
			} else {
				if (Vector3.Distance(VectorYNormalize(transform.position), VectorYNormalize(hits[0].transform.position)) <= 1.4f) {
					state = State.Attacking;
				} else if (curLimit >= rLimit) {
					PathfindToPlayer(hits[0].transform);
					curLimit = 0f;
					//Debug.Log("Pathing to " + hits[0].transform.position);
				} else {
					curLimit += Time.fixedDeltaTime;
				}
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
			if (dir != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(dir);
				rb.MoveRotation(targetRotation);
			}
		} else {
			velocity.x = 0;
			velocity.z = 0;
			GetAimlessDestination();
		}
		if (state == State.Attacking) {
			animator.Play("Attack");
			return;
		}
		if (velocity.magnitude > 0.05f) {
			animator.SetBool("Moving", true);
		} else {
			animator.SetBool("Moving", false);
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
				pathIdx = 0;
				return;
			}
		}
	}
	
	private Vector2 VectorYNormalize(Vector3 v3) {
		return new Vector2(v3.x, v3.z);
	}
	
	public virtual void AnimationEndHelper() {
		state = State.Wandering;
	}
	
	public virtual void SetBound(bool setTo) {
		if (setTo && state == State.Attacking) {
			animator.Play("EmptyAnimanim");
			state = State.Bounded;
			return;
		}
 		state = setTo ? State.Bounded : State.Wandering;
		Debug.Log("Enemy State: " + state);
	}
	
	public void takeDamage(float dmg, float stunLength) {
		if (stunLength > stunDuration) {
			stunDuration = stunLength;
		}
		HP -= dmg;
	}
	
	public bool AttemptConsume() {
		if (HP <= Devourable) {
			Destroy(gameObject);
			return true;
		}
		return false;
	}
	
	public virtual void UnlockPower() {
		
	}
}
