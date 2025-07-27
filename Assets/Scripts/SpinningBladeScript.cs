using UnityEngine;
using UnityEngine.AI;



public class SpinningBladeScript : EnemyAI
{
	[SerializeField] private GameObject unlock;
	[SerializeField] private BladeHitbox hitbox;
	
    private enum State {
		Deactivated,
		Stunned,
		Activating,
		Chasing,
		Dead
	}
	
	private State bladeState;
	[SerializeField] private float maxSpeed = 4f;
	private float curSpeed = 0f;
	
	[SerializeField] private float rateLimit = 0.2f;
	private float rateTimer;
	
	protected override void Start()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		bladeState = State.Deactivated;
	}
	
	protected override void FixedUpdate() {
		if (bladeState == State.Dead) return;
		RaycastHit hit;
		bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f, LayerMask.GetMask("Ground"));
		if (HP <= 0 && bladeState != State.Dead) {
			animator.Play("Death");
			bladeState = State.Dead;
			return;
		}
		
		if (bladeState == State.Stunned) {
			stunDuration -= Time.fixedDeltaTime;
			if (stunDuration <= 0f) {
				stunDuration = 0f;
				bladeState = State.Deactivated;
			} else {
				return;
			}
		}
		
		Collider[] hits = Physics.OverlapSphere(transform.position, 6f, LayerMask.GetMask("Player"));
		if (hits.Length <= 0) {
			curSpeed = 0f;
			if (bladeState == State.Chasing) {
				bladeState = State.Deactivated;
				hitbox.DisableHitbox();
				animator.Play("Idle");
			}
		} else {
			PathfindToPlayer(hits[0].transform);
			Debug.Log(path.corners.Length);
			if (path.corners.Length != 2) {
				path = new NavMeshPath();
				if (bladeState == State.Chasing) {
					bladeState = State.Deactivated;
					hitbox.DisableHitbox();
				}
			} else {
				if (bladeState == State.Deactivated) {
					bladeState = State.Activating;
					animator.Play("Activate");
				} else if (bladeState == State.Chasing) {
					Vector3 dir = (path.corners[1] - transform.position);
					dir.y = 0f;
					if (dir.magnitude < 0.8f) {
						bladeState = State.Deactivated;
						return;
					}
					if (curSpeed < maxSpeed) {
						curSpeed += 2f * Time.fixedDeltaTime;
						if (dir != Vector3.zero) {
							Quaternion targetRotation = Quaternion.LookRotation(dir);
							rb.MoveRotation(targetRotation);
						}
					} else {
						curSpeed = maxSpeed;
					}
					Vector3 dest = transform.position + transform.forward.normalized * Time.fixedDeltaTime * (curSpeed + 1);
					NavMeshHit navhit;
					bool check = NavMesh.SamplePosition(dest - new Vector3(0, 1.1f, 0), out navhit, 0.2f, NavMesh.AllAreas);
					Debug.Log(check);
					if (check) {
						rb.MovePosition(transform.position + transform.forward.normalized * Time.fixedDeltaTime * (curSpeed + 1));
					} else {
						curSpeed = maxSpeed / 2f;
					}
				}
			}
		}
	}
	
	public void ActivationHelper() {
		bladeState = State.Chasing;
		hitbox.EnableHitbox();
	}
	
	public void HitboxHelper(bool shelled) {
		curSpeed = 0f;
		bladeState = State.Stunned;
		animator.Play("Idle");
		if (shelled) {
			stunDuration = 3f;
		} else {
			stunDuration = 1f;
		}
	}
	
	protected override void PathfindToPlayer(Transform player) {
		for (int i = 0; i < 10; i++) {
			if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath(transform.position - new Vector3(0, 1.1f, 0), hit.position, NavMesh.AllAreas, path);
				target = player.GetComponent<PlayerStateMachine>();
				pathIdx = 0;
				Debug.Log(path.corners.Length);
				return;
			}
		}
	}
}
