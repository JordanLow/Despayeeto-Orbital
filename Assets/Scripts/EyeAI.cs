using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EyeAI : EnemyAI
{
    [SerializeField] private GameObject unlock;
	[SerializeField] private List<GameObject> lenses;
	
	private enum State {
		Wandering,
		Hunting,
		Lasering,
		Chasing,
		Dead
	}
	
	private State eyeState;
	
	private EnemyAI absorbTarget; 
	private int lens = 0; // 0 = No Lens, 1 = Large Lens, 2 = Radical Lens, 3 = Sticky Lens
	
	[SerializeField] private float rateLimit = 0.3f;
	private float rateTimer;
	
	protected override void Start()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		path = new NavMeshPath();
		velocity = Vector3.zero;
		eyeState = State.Wandering;
		
		GetAimlessDestination();
	}
	
	protected override void FixedUpdate()
	{
		if (eyeState == State.Dead) return;
		RaycastHit groundHit;
		bool nearGround = Physics.Raycast(transform.position, Vector3.down, out groundHit, 1.3f, LayerMask.GetMask("Ground"));
		if (HP <= 0 && eyeState != State.Dead) {
			eyeState = State.Dead;
			return;
		}
		if (eyeState == State.Lasering) {
			return;
		}
		if (eyeState == State.Wandering) {
			
			Collider[] hits = Physics.OverlapSphere((transform.position - new Vector3(0, 1.3f, 0)), 6f, LayerMask.GetMask("Player"));
			
			if (hits.Length > 0) {
				eyeState = State.Chasing;
				if (lens == 0) {
					Collider[] enemHits = Physics.OverlapSphere((transform.position - new Vector3(0, 1.3f, 0)), 10f, LayerMask.GetMask("Enemy"));
					foreach (Collider hit in enemHits) {
						if (hit.GetComponent<EnemyAI>().type > 0 && hit.GetComponent<EnemyAI>().type < 4) {
							absorbTarget = hit.GetComponent<EnemyAI>();
							PathfindToPrey(absorbTarget.transform);
							eyeState = State.Hunting;
							break;
						}
					}
				}
			}			
		}
		
		if (eyeState == State.Hunting) {
			
			if (absorbTarget == null) {
				eyeState = State.Wandering;
				return;
			}
			
			if ((absorbTarget.transform.position - transform.position).magnitude < 2f) {
				lens = absorbTarget.type;
				lenses[lens-1].SetActive(true);
				Destroy(absorbTarget.gameObject);
				eyeState = State.Wandering;
				return;
			}
			
			if (rateTimer >= rateLimit) {
				rateTimer = 0f;
				PathfindToPrey(absorbTarget.transform);
			} else {
				rateTimer += Time.fixedDeltaTime;
			}
		}
		
		if (eyeState == State.Chasing) {
			if (rateTimer >= rateLimit) {
				Collider[] hits = Physics.OverlapSphere((transform.position - new Vector3(0, 1.3f, 0)), 6f, LayerMask.GetMask("Player"));
				
				if (hits.Length > 0) {
					rateTimer = 0f;
					if ((hits[0].transform.position - transform.position).magnitude < 3.5f) {
						eyeState = State.Lasering;
						Vector3 dir = hits[0].transform.position - transform.position;
						dir.y = 0;
						Quaternion targetRotation = Quaternion.LookRotation(dir);
						rb.MoveRotation(targetRotation);
						switch (lens) {
							case 1:
								animator.Play("Large Laser");
								break;
							case 2:
								animator.Play("Radical Laser");
								break;
							case 3:
								animator.Play("Sticky Laser");
								break;
							default:
								animator.Play("Laser");
								break;
						}
						return;
					} else {
						PathfindToPlayer(hits[0].transform);
					}
				} else {
					eyeState = State.Wandering;
					GetAimlessDestination();
				}
			} else {
				rateTimer += Time.fixedDeltaTime;
			}
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
		Vector3 vel = transform.forward.normalized * Time.fixedDeltaTime * speed;
		if (nearGround) vel.y = -1 * Physics.gravity.y * Time.fixedDeltaTime;
		rb.MovePosition(transform.position + vel);
	}
	
	public void LaserHelper() {
		eyeState = State.Wandering;
	}
	
	
	protected override void GetAimlessDestination() {
		for (int i = 0; i < 10; i++) {
			Vector3 randPoint = (transform.position - new Vector3(0, 1.3f, 0)) + Random.insideUnitSphere * 5f;
			if (UnityEngine.AI.NavMesh.SamplePosition(randPoint, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas)) {
				UnityEngine.AI.NavMesh.CalculatePath((transform.position - new Vector3(0, 1.3f, 0)), hit.position, UnityEngine.AI.NavMesh.AllAreas, path);
				pathIdx = 0;
				return;
			}
		}
	}
	
	protected override void PathfindToPlayer(Transform player) {
		for (int i = 0; i < 10; i++) {
			if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath((transform.position - new Vector3(0, 1.3f, 0)), hit.position, NavMesh.AllAreas, path);
				pathIdx = 0;
				// Debug.Log("Path Found");
				return;
			}
		}
		// Debug.Log("Path Not Found");
	}
	
	protected void PathfindToPrey(Transform prey) {
		for (int i = 0; i < 10; i++) {
			if (NavMesh.SamplePosition(prey.position, out NavMeshHit hit, 2f, NavMesh.AllAreas)) {
				NavMesh.CalculatePath(transform.position - new Vector3(0, 1.3f, 0), hit.position, NavMesh.AllAreas, path);
				pathIdx = 0;
				return;
			}
		}
	}
}
