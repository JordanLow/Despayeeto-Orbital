using UnityEngine;
using UnityEngine.AI;

// Refactor into extensible for different enemy types in the future
public class EnemyAI : MonoBehaviour
{
	[SerializeField] private float speed;
	
    protected Rigidbody rb;
	protected NavMeshPath path;
	protected int pathIdx;
	protected Vector3 velocity;
	protected Animator animator;
	
	[SerializeField] protected float HP = 20f;
	[SerializeField] protected float Devourable = 10f;
	[SerializeField] public float heightOffset = 0.45f;
	
	private enum State {
		Wandering,
		Chasing,
		Attacking,
		Bounded,
		Stunned,
		Yanked,
		Dead
	}
	
	private State state;
	private float curLimit = 0f;
	private float rLimit = 0.4f;
	private float timeDied = 2f;
	private Vector3 yankTo;
	private Vector3 yankFrom;
	private float yankTime = 0f;
	public float stunDuration = 0f;
	protected PlayerStateMachine target;
	
	protected virtual void Start()
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
		if (state == State.Dead) {
			timeDied -= Time.fixedDeltaTime;
			if (timeDied <= 0f) Destroy(gameObject);
			return;
		}
		if (HP <= 0f) {
			state = State.Dead;
			animator.Play("Deflate");
			rb.detectCollisions = false;
			target?.crowd.Remove(this);
			return;
		}
		if (state == State.Yanked) {
			yankTime += Time.fixedDeltaTime;
			if (yankTime >= 0.33f) {
				yankTime = 0f;
				state = State.Wandering;
				return;
			}
			rb.MovePosition(Vector3.Lerp(yankFrom, yankTo + ((yankFrom - yankTo).normalized * 0.6f), yankTime * 3f));
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
		float mod = 1f;
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.1f, LayerMask.GetMask("Ground"));
		if (state == State.Wandering) {
			Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
			if (hits.Length > 0) {
				// Debug.Log("Player Detected");
				Transform nearestPlayer = hits[0].transform;
				state = State.Chasing;
				curLimit = 0f;
				PathfindToPlayer(nearestPlayer);
			}
		} else if (state == State.Chasing) {
			Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
			if (hits.Length <= 0) {
				// Debug.Log("Player lost");
				target.crowd.Remove(this);
				state = State.Wandering;
				GetAimlessDestination();
			} else {
				if (!target.crowd.Contains(this)) {
					if (target.crowd.Count <= 5) {
						target.crowd.Add(this);
					} else if (Vector3.Distance(VectorYNormalize(transform.position), VectorYNormalize(hits[0].transform.position)) <= 3f) {
						mod = 0f;
					}
				}
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
		Vector3 dest = rb.position + velocity * Time.fixedDeltaTime * mod;
		if (!isGrounded && velocity.y > Physics.gravity.y) velocity.y += Physics.gravity.y * Time.fixedDeltaTime;
		if (isGrounded) {
			dest.y = hit.point.y + 0.01f;
            rb.MovePosition(dest);
			velocity.y = 0;
		} else {
			rb.MovePosition(dest);
		}
	}
	
	protected virtual void GetAimlessDestination() {
		for (int i = 0; i < 10; i++) {
			Vector3 randPoint = transform.position + Random.insideUnitSphere * 5f;
			if (NavMesh.SamplePosition(randPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path);
				pathIdx = 0;
				return;
			}
		}
	}
	
	protected virtual void PathfindToPlayer(Transform player) {
		for (int i = 0; i < 10; i++) {
			if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path);
				target = player.GetComponent<PlayerStateMachine>();
				pathIdx = 0;
				return;
			}
		}
	}
	
	protected Vector2 VectorYNormalize(Vector3 v3) {
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
	
	public virtual void takeDamage(float dmg, float stunLength, PlayerStateMachine src) {
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
	
	public virtual void AttemptYank(PlayerStateMachine src) {
		animator.Play("EmptyAnimanim"); // Placeholder for actual stun animation
		state = State.Yanked;
		yankFrom = rb.position;
		yankTo = src.transform.position;
	}
	
	public virtual void AttemptGrab(PlayerStateMachine src) {
		src.animationCancelled = true;
		src.movementStates.Bound.boundedEnemy = this;
		src.actionStates.Bounding.boundedEnemy = this;
		src.MovementTransition(src.movementStates.Bound);
		src.ActionTransition(src.actionStates.Bounding);
		src.movementStates.Bound.boundedEnemy.SetBound(true);
	}
	
	public virtual void UnlockPower() { }
}
