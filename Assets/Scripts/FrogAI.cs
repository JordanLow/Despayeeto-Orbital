using UnityEngine;
using UnityEngine.AI;

public class FrogAI : EnemyAI
{
    [SerializeField] private GameObject unlock;
	[SerializeField] private GameObject marker;
	public AnimationCurve hop;
	public AnimationCurve leap;
	public int strike;
	
	private enum State {
		Wandering,
		Chasing,
		Hopping,
		Leaping,
		Striking,
		Dead
	}
	
	private State frogState;
	private float rateLimit = 0.5f;
	private Vector3 posMem;
	
	private float animTime = 0f;
	private float hopDuration = 1f;
	private float leapDuration = 2.5f;
	private float strikeDuration = 4f;

	// RN Leap Collide just flips the frog
	// Code the actual rebound later
	// private bool hitHelper = false;
	
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
			new Keyframe(0.5f, 0f),
			new Keyframe(1f, -1f)
		);
		leap = new AnimationCurve(
			new Keyframe(0f, 0f),
			new Keyframe(0.4f, 0f),
			new Keyframe(0.5f, 1f),
			new Keyframe(0.6f, 0f),
			new Keyframe(1f, 0f)
		);
		strike = 0;
		
		GetAimlessDestination();
	}
	
	protected override void FixedUpdate() {
		if (HP <= 0 && frogState != State.Dead) {
			frogState = State.Dead;
			return;
		}
		
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 0.6f, LayerMask.GetMask("Ground"));
		
		if (frogState == State.Hopping) {
			animTime += Time.fixedDeltaTime;
			float yOffset = hop.Evaluate(animTime / hopDuration);
			Vector3 move = transform.forward.normalized * Time.fixedDeltaTime;
			move.y += yOffset * Time.deltaTime;
			if (isGrounded && move.y < 0f) move.y = hit.point.y - transform.position.y + 0.51f;
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
			if (isGrounded && move.y < 0f) move.y = hit.point.y - transform.position.y + 0.51f;
			rb.MovePosition(rb.position + move);
			Debug.Log(move);

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
			} else if (strike == 2) {
					Vector3 move = new Vector3(0, -6f * Time.fixedDeltaTime, 0);
					if (isGrounded) move.y = hit.point.y - transform.position.y + 0.51f;
					rb.MovePosition(rb.position + move);
			}
			
			if (animTime >= strikeDuration) {
				marker.transform.localScale = new Vector3(1, 0.01f, 1);
				animTime = 0f;
				strike = 0;
				frogState = State.Wandering;
				transform.position = posMem;
			}
			
			return;
		}
		
		if (!isGrounded) {
			rb.MovePosition(transform.position + new Vector3(0, Physics.gravity.y * Time.fixedDeltaTime, 0));
		} else if (isGrounded) {
			Vector3 dest = hit.point;
			dest.y += 0.51f;
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
				} else if (dist >= 2f && dist <= 3.4f) {
					// If Player within attack range, shift to attack state
					 frogState = State.Leaping;
					// Ask later how to switch attack logic
					// frogState = State.Striking;
					// posMem = transform.position;
				} else {
					
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
			// Play Hop Anim
			frogState = State.Hopping;
		}
	}
	
	public override void UnlockPower() {
		unlock.SetActive(true);
	}
	
	public override void AttemptYank(PlayerStateMachine src) { }
	
	public override void AttemptGrab(PlayerStateMachine src) { }

	public void LeapCollideHelper() {
		// hitHelper = true;
		transform.rotation = Quaternion.LookRotation(transform.forward * -1f);
	}
	
	public void StrikeHelperAscend() {
		strike = 1;
		Collider[] hits = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Player"));
		if (hits.Length > 0) {
			transform.position = hits[0].transform.position;
		}
	}
	
	public void StrikeHelperDescend() {
		strike = 2;
		transform.position = transform.position + new Vector3(0f, 3f, 0f);
	}
}
