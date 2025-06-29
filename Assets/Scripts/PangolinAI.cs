using UnityEngine;
using UnityEngine.AI;



public class PangolinAI : EnemyAI
{
    [SerializeField] private GameObject unlock;
	
	public AnimationCurve scratch;
	public AnimationCurve barge;
	
	private enum State {
		Wandering,
		Chasing,
		Scales,
		Scratching,
		Barge,
		Stunned,
		Dead
	}
	
	private State pangState;
	[SerializeField] private float rateLimit = 0.3f;
	private float rateTimer;
	
	private float animTime;
	private float scratchDuration = 1f;
	private float bargeDuration = 1f;
	
	private float scaleCD = 0f;
	
	protected override void Start()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		velocity = Vector3.zero;
		pangState = State.Wandering;
		scratch = new AnimationCurve(
			new Keyframe(0, 0),
			new Keyframe(0.33f, 0),
			new Keyframe(0.5f, 1f),
			new Keyframe(0.75f, 1f),
			new Keyframe(1f, 0)
		);
		barge = new AnimationCurve(
			new Keyframe(0, 0),
			new Keyframe(0.58f, 0),
			new Keyframe(0.65f, 1f),
			new Keyframe(0.9f, 1f),
			new Keyframe(1f, 0)
		);

		GetAimlessDestination();
	}
	
	protected override void FixedUpdate() {
		
		if (HP <= 0 && pangState != State.Dead) {
			animator.Play("Idle");
			pangState = State.Dead;
			return;
		}
		if (pangState == State.Dead) return;
		
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.6f, LayerMask.GetMask("Ground"));
		
		if (isGrounded) {
			transform.position = new Vector3(transform.position.x, hit.point.y + 0.51f, transform.position.z);
		}
		
		if (pangState == State.Stunned) {
			if (!isGrounded) rb.MovePosition(transform.position + new Vector3(0, Physics.gravity.y * Time.fixedDeltaTime, 0));
			stunDuration -= Time.fixedDeltaTime;
			if (stunDuration <= 0f) {
				stunDuration = 0f;
				pangState = State.Wandering;
			} else {
				return;
			}
		}
		
		if (pangState == State.Scales) {
			return;
		}
		
		if (pangState == State.Scratching) {
			animTime += Time.fixedDeltaTime;
			float speed = scratch.Evaluate(animTime / scratchDuration);
			Vector3 move = Vector3.zero;
			if (speed > 0f) move = transform.forward.normalized * 7f * (1f + speed) * Time.fixedDeltaTime;
			if (isGrounded) {
				move.y = hit.point.y - transform.position.y + 0.51f;
			}
			rb.MovePosition(rb.position + move);

			if (animTime >= scratchDuration) {
				animTime = 0f;
				pangState = State.Wandering;
			}
			
			return;
		}
		
		if (pangState == State.Barge) {
			animTime += Time.fixedDeltaTime;
			float speed = barge.Evaluate(animTime / bargeDuration);
			Vector3 move = Vector3.zero;
			if (speed > 0f) move = transform.forward.normalized * 3f * (1f + speed) * Time.fixedDeltaTime;
			if (isGrounded) {
				move.y = hit.point.y - transform.position.y + 0.51f;
			}
			rb.MovePosition(rb.position + move);

			if (animTime >= scratchDuration) {
				animTime = 0f;
				pangState = State.Wandering;
			}
			
			return;
		}
		
		if (scaleCD > 0f) {
			scaleCD -= Time.fixedDeltaTime;
			if (scaleCD <= 0f) scaleCD = 0f;
		}
		
		if (pangState == State.Chasing || pangState == State.Wandering) {
			if (rateTimer >= rateLimit) {
				rateTimer = 0f;
				// Check if Player within vicinity
				Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
				if (hits.Length <= 0) {
					if (pangState == State.Chasing) GetAimlessDestination();
					if (pangState == State.Chasing) pangState = State.Wandering;
				} else {
					PathfindToPlayer(hits[0].transform);
					if (pangState == State.Wandering) pangState = State.Chasing;
					
					float dist = Vector3.Distance(VectorYNormalize(transform.position), VectorYNormalize(hits[0].transform.position));
					Vector3 dir = hits[0].transform.position - transform.position;
					dir.y = 0;
					transform.rotation = Quaternion.LookRotation(dir);
					if (dist <= 2f) {
						// Scales if available
						if (scaleCD == 0f) {
							pangState = State.Scales;
							// Play scale animation
						} else {
							pangState = State.Barge;
							animator.Play("Barge");
						}
					} else if (dist >= 2f && dist <= 4f) {
						pangState = State.Scratching;
						animator.Play("Scratch");
					} else {
						if (pangState == State.Scales) pangState = State.Wandering;
					}
				}
			} else {
				rateTimer += Time.fixedDeltaTime;
			}
			
			if (path != null && pathIdx < path.corners.Length) {
				Vector3 dir = (path.corners[pathIdx] - transform.position);
				dir.y = 0f;
				if (dir.magnitude < 0.2f) {
					pathIdx++;
				}
				if (dir != Vector3.zero) {
					Quaternion targetRotation = Quaternion.LookRotation(dir);
					rb.MoveRotation(targetRotation);
				}
			} else {
				GetAimlessDestination();
			}
			Vector3 vel = transform.forward.normalized * Time.fixedDeltaTime;
			if (!isGrounded) vel.y = Physics.gravity.y * Time.fixedDeltaTime;
			rb.MovePosition(transform.position + vel);
		} else {
			if (!isGrounded) rb.MovePosition(transform.position + new Vector3(0, Physics.gravity.y * Time.fixedDeltaTime, 0));
		}
	}
	
	public override void UnlockPower() {
		unlock.SetActive(true);
	}
	
	public override void AttemptYank(PlayerStateMachine src) { }
	
	public override void AttemptGrab(PlayerStateMachine src) { }
	
	public override void takeDamage(float dmg, float stunLength, PlayerStateMachine src) {
		if (pangState == State.Scales) {
			if (Vector3.Angle(transform.forward, src.transform.position - transform.position) <= 65f) return;
			pangState = State.Stunned;
			stunDuration = 1.5f;
			scaleCD = 7f;
		}
		HP -= dmg;
	}
	
	public void BargeReaction(PlayerStateMachine victim) {
		victim.Reaction(new Vector3(0, 9f, 0), 1f);
	}
	
	public void SlashReaction(PlayerStateMachine victim) {
		Vector3 toTarget = (victim.transform.position - transform.position).normalized;
		float side = Vector3.Dot(toTarget, transform.right);
		Vector3 direction = side > 0 ? transform.right : -transform.right;
		victim.Reaction(direction * 4f, 0.4f);
	}
}
