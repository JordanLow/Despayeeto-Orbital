using UnityEngine;
using UnityEngine.AI;



public class FrogAI : EnemyAI
{
    [SerializeField] private GameObject unlock;
	[SerializeField] private GameObject tempUnlock;
	[SerializeField] private GameObject tempUnlock2;
	[SerializeField] private GameObject marker;
	public AnimationCurve hop;
	public AnimationCurve leap;
	public AnimationCurve rebound;
	private int strike;
	
	private enum State {
		Wandering,
		Chasing,
		Hopping,
		Leaping,
		Striking,
		Rebounding,
		Dead
	}
	
	private State frogState;
	private float rateLimit = 0.5f;
	private Vector3 posMem;
	private bool hopStart;
	
	private float animTime = 0f;
	private float hopDuration = 1.5f;
	private float leapDuration = 5f;
	private float reboundDuration = 1f;
	private float strikeDuration = 8.25f;
	
	protected override void Start()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		velocity = Vector3.zero;
		frogState = State.Wandering;
		
		hop = new AnimationCurve(
			new Keyframe(0f, 1f),
			new Keyframe(0.4f, 0f),
			new Keyframe(0.8f, -1f),
			new Keyframe(0.81f, 0f),
			new Keyframe(1f, 0f)
		);
		leap = new AnimationCurve(
			new Keyframe(0f, 0f),
			new Keyframe(0.43f, 0f),
			new Keyframe(0.53f, 1f),
			new Keyframe(0.63f, 0f),
			new Keyframe(1f, 0f)
		);
		rebound = new AnimationCurve(
			new Keyframe(0f, 1f),
			new Keyframe(0.5f, 0f),
			new Keyframe(1f, -1f)
		);
		strike = -1;
		
		GetAimlessDestination();
	}
	
	protected override void FixedUpdate() {
		if (HP <= 0 && frogState != State.Dead) {
			animator.Play("Idle");
			Vector3 newScale = transform.localScale;
			newScale.y = newScale.y * -1f;
			transform.localScale = newScale;
			transform.position = transform.position + new Vector3(0, 1, 0);
			frogState = State.Dead;
			return;
		}
		if (frogState == State.Dead) return;
		
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position + new Vector3(0f, 1f, 0f), Vector3.down, out hit, 1.1f, LayerMask.GetMask("Ground"));
		
		if (frogState == State.Hopping) {
			if (!hopStart) return;
			animTime += Time.fixedDeltaTime;
			float yOffset = hop.Evaluate(animTime / hopDuration);
			Vector3 move = transform.forward.normalized * Time.fixedDeltaTime;
			move.y += yOffset * Time.deltaTime;
			if (isGrounded && move.y < 0f) {
				animator.Play("Land");
				move.y = hit.point.y - transform.position.y + 0.01f;
			}
			rb.MovePosition(rb.position + move);

			if (animTime >= hopDuration) {
				animTime = 0f;
				frogState = State.Wandering;
			}
			
			return;
		}
		
		if (frogState == State.Leaping) {
			animTime += Time.fixedDeltaTime;
			float speed = leap.Evaluate(animTime / leapDuration);
			Vector3 move = Vector3.zero;
			if (speed > 0f) move = transform.forward.normalized * 5f * Time.fixedDeltaTime;
			if (isGrounded) {
				move.y = hit.point.y - transform.position.y + 0.01f;
			}
			rb.MovePosition(rb.position + move);

			if (animTime >= leapDuration) {
				animTime = 0f;
				frogState = State.Wandering;
			}
			
			return;
		}
		
		if (frogState == State.Striking) {
			animTime += Time.fixedDeltaTime;
			if (strike == 0) {
				Vector3 move = new Vector3(0, 6f * Time.fixedDeltaTime, 0);
				rb.MovePosition(rb.position + move);
			} else if (strike == 1) {
				marker.transform.localScale += new Vector3(2*Time.fixedDeltaTime / 2.25f, 0, 2*Time.fixedDeltaTime / 2.25f);
				Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
				if (hits.Length > 0) {
					transform.position = hits[0].transform.position;
				}
			} else if (strike == 2) {
					Vector3 move = new Vector3(0, -3f * Time.fixedDeltaTime, 0);
					if (isGrounded) move.y = hit.point.y - transform.position.y + 0.01f;
					Debug.Log(move);
					rb.MovePosition(rb.position + move);
			}
			
			if (animTime >= strikeDuration) {
				marker.transform.localScale = new Vector3(1, 0.01f, 1);
				animTime = 0f;
				strike = -1;
				Vector3 dir = posMem - transform.position;
				dir.y = 0;
				transform.rotation = Quaternion.LookRotation(dir);
				frogState = State.Rebounding;
			}
			
			return;
		}
		
		if (frogState == State.Rebounding) {
			animTime += Time.fixedDeltaTime;
			float yOffset = rebound.Evaluate(animTime / reboundDuration);
			Vector3 move = transform.forward.normalized * 2f * Time.fixedDeltaTime;
			move.y += yOffset * Time.deltaTime;
			if (isGrounded && move.y < 0f) {
				animator.Play("Land");
				move.y = hit.point.y - transform.position.y + 0.01f;
			}
			rb.MovePosition(rb.position + move);

			if (animTime >= reboundDuration) {
				animTime = 0f;
				frogState = State.Wandering;
			}
			
			return;
		}
		
		if (!isGrounded) {
			rb.MovePosition(transform.position + new Vector3(0, Physics.gravity.y * Time.fixedDeltaTime, 0));
		} else if (isGrounded) {
			Vector3 dest = hit.point;
			dest.y += 0.01f;
            rb.MovePosition(dest);
		}

		if (frogState == State.Leaping || frogState == State.Striking) return;
		
		
		if (rateLimit >= 0f) {
			rateLimit -= Time.fixedDeltaTime;
			return;
		}
		rateLimit = 0.5f;
		
		
		
		bool reverse = false;
		
		if (frogState == State.Wandering || frogState == State.Chasing) {
			// Check if Player within vicinity
			Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
			if (hits.Length <= 0) {
				// If not, set to random path and set rotation.
				GetAimlessDestination();
			} else {
				// If so, check proximity and set rotation accordingly
				float dist = Vector3.Distance(VectorYNormalize(transform.position), VectorYNormalize(hits[0].transform.position));
				PathfindToPlayer(hits[0].transform);
				// Debug.Log(dist);
				if (dist <= 2f) {
					// Hop Backwards
					reverse = true;
				} else if (dist >= 2f && dist <= 4f) {
					if (Random.value <= 0.7f) {
						// If Player within attack range, shift to attack state
						frogState = State.Leaping;
					} else {
						// Ask later how to switch attack logic
						frogState = State.Striking;
						posMem = transform.position;
					}
				}
			}
		}
		if (path != null && pathIdx < path.corners.Length) {
			Vector3 dir = (path.corners[pathIdx + 1] - transform.position);
			dir.y = 0f;
			if (dir.magnitude < 0.2f) {
				pathIdx++;
			}
			if (dir != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(dir);
				if (reverse) {
					targetRotation = Quaternion.LookRotation(dir * -1f);
				}
				rb.MoveRotation(targetRotation);
			}
		} else {
			GetAimlessDestination();
		}
		
		// If in attack state, launch attack
		// If not, hop to it!
		if (frogState == State.Leaping) {
			animator.Play("Leap");
		} else if (frogState == State.Striking) {
			animator.Play("Strike");			
		} else {
			animator.Play("Hop");
			hopStart = false;
			frogState = State.Hopping;
		}
	}
	
	public override void UnlockPower() {
		unlock.SetActive(true);
		tempUnlock.SetActive(true);
		tempUnlock2.SetActive(true);
	}
	
	public override void AttemptYank(PlayerStateMachine src) { }
	
	public override void AttemptGrab(PlayerStateMachine src) { }

	public void LeapCollideHelper() {
		frogState = State.Rebounding;
		animTime = 0f;
		transform.rotation = Quaternion.LookRotation(transform.forward.normalized * -1f);
	}
	
	public void StrikeHelper() {
		strike = 0;
	}
	
	public void StrikeHelperAscend() {
		strike = 1;
		Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
		if (hits.Length > 0) {
			transform.position = hits[0].transform.position;
		} else {
			transform.position = transform.position - new Vector3(0, 4.8f, 0);
		}
	}
	
	public void StrikeHelperDescend() {
		strike = 2;
		transform.position = transform.position + new Vector3(0f, 2f, 0f);
	}
	
	public void HopHelper() {
		hopStart = true;
	}
	
	public void LeapReaction(PlayerStateMachine victim) {
		victim.Reaction(transform.forward, 0.3f);
	}
}
