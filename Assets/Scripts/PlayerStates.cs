using UnityEngine;
using System;
using Unity.Cinemachine;

public abstract class PlayerState
{
    public virtual void Enter() { }
	public virtual void Exit() { }
	public virtual void Tick() { }
	
	public virtual Command FilterCommand(Command command) { return command; }
	
	public virtual void OnJumpCommand() { }
	public virtual void OnJabCommand() { }
	public virtual void OnAttenuateCommand() { }
	public virtual void OnBoundCommand() { }
	public virtual void OnConsumeCommand() { }
	public virtual void OnChargeRollCommand() { }
	public virtual void OnSlamCommand() { }
	public virtual void OnSpineCommand() { }
	public virtual void OnImpaleCommand() { } 
	public virtual void OnGrabCommand() { }
	public virtual void OnYankCommand() { }
}

public abstract class PlayerMovementState : PlayerState
{
	protected readonly PlayerStateMachine FSM;
	
	public PlayerMovementState(PlayerStateMachine ctx) {
		this.FSM = ctx;
	}
	
	public override void Tick() {
		Vector3 cameraForward = FSM.cam.transform.forward; 
		cameraForward.y = 0;
        Vector3 cameraRight = FSM.cam.transform.right; 
		cameraRight.y = 0;
		Vector3 inputDirection = FSM.movementAllowed ? FSM.inputDirection : Vector3.zero;
		Vector3 currentVelocity = FSM.currentVelocity;
		if (currentVelocity.y > Physics.gravity.y) currentVelocity.y += Physics.gravity.y * Time.deltaTime;
		inputDirection = inputDirection.z * cameraForward + inputDirection.x * cameraRight;
		inputDirection.y = currentVelocity.y;
		FSM.currentVelocity = inputDirection;
		FSM.controller.Move(FSM.currentVelocity * Time.deltaTime);
		inputDirection.y = 0;
		if (inputDirection == Vector3.zero) return;
		FSM.transform.rotation = Quaternion.LookRotation(inputDirection);
	}
}

public abstract class PlayerActionState : PlayerState
{
	protected readonly PlayerStateMachine FSM;
	
	public PlayerActionState(PlayerStateMachine ctx) {
		this.FSM = ctx;
	}
	
	public virtual void ModeSet(int mode) { }
}

public abstract class PlayerMetaState : PlayerState
{
	protected readonly PlayerStateMachine FSM;
	
	public PlayerMetaState(PlayerStateMachine ctx) {
		this.FSM = ctx;
	}
}

public class PlayerGroundedState : PlayerMovementState
{
	public PlayerGroundedState(PlayerStateMachine ctx) : base(ctx) { }
	private bool isJumping = false;
	public bool slamQueued = false;
	
	public override void Exit() {
		isJumping = false;
	}

	public override void Tick() {
		if (slamQueued) {
			FSM.JumpHelper();
			FSM.ActionTransition(FSM.actionStates.Slamming);
			FSM.MovementTransition(FSM.movementStates.Slam);
			slamQueued = false;
			return;
		}
		base.Tick();
		if (!Physics.CheckSphere(FSM.transform.position, 0.1f, LayerMask.GetMask("Ground"))) {
		//if (!FSM.controller.isGrounded) {
			FSM.MovementTransition(FSM.movementStates.Airborne);
			FSM.animator.SetBool("GroundedMoving", false);
		} else {
			Vector3 planarVelo = FSM.currentVelocity;
			planarVelo.y = 0;
			if (planarVelo.magnitude >= 0.2f) {
				FSM.animator.SetBool("GroundedMoving", true);
			} else {
				FSM.animator.SetBool("GroundedMoving", false);
			}
		}
	}
	
	public override Command FilterCommand(Command command) {
		if (isJumping) return new VoidCommand();
		if (command is SlamCommand) {
			return new ConsumeCommand();
		}
		return command;
	}

	public override void OnJumpCommand() {
		isJumping = true;
		FSM.animator.Play("BounceSquish", 1);
	}
}

public class PlayerAimingState : PlayerMovementState
{
	private float currentPitch;
	
	public PlayerAimingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.animator.SetBool("GroundedMoving", false);
		currentPitch = 0f;
		FSM.aimAnchor.transform.localEulerAngles = Vector3.zero;
		Vector3 camForward = FSM.cam.transform.forward;
		camForward.y = 0;
		FSM.transform.rotation = Quaternion.LookRotation(camForward);
	}
	
	public override void Tick() {
		// Aim the Camera/Player Rotation
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");
        currentPitch -= mouseY * 2f;
        currentPitch = Mathf.Clamp(currentPitch, -20f, 30f);
        FSM.aimAnchor.transform.localEulerAngles = new Vector3(currentPitch, 0f, 0f);
		FSM.transform.Rotate(Vector3.up * mouseX * 2f);
		Vector3 inputDirection = FSM.movementAllowed ? FSM.inputDirection : Vector3.zero;
		Vector3 currentVelocity = FSM.currentVelocity;
		if (currentVelocity.y > Physics.gravity.y) currentVelocity.y += Physics.gravity.y * Time.deltaTime;
		inputDirection = inputDirection.z * FSM.transform.forward + inputDirection.x * FSM.transform.right;
		inputDirection.y = currentVelocity.y;
		FSM.currentVelocity = inputDirection;
		FSM.controller.Move(FSM.currentVelocity * Time.deltaTime * 0.4f);
		/* // From Player  
		
		RaycastHit[] hits = Physics.SphereCastAll(FSM.transform.position, 1.4f, FSM.cam.transform.forward, 40f, LayerMask.GetMask("Enemy"));
		Vector3 aimPoint = FSM.aimCam.State.ReferenceLookAt;
		if (hits.Length > 0) {
			Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
			aimPoint = hits[0].transform.position;
		} */
		
		// From Camera
		Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
		Ray ray = Camera.main.ScreenPointToRay(screenCenter);
		Vector3 aimPoint = FSM.aimCam.State.ReferenceLookAt;
		RaycastHit[] hits = Physics.SphereCastAll(ray, 1.2f, 40f, LayerMask.GetMask("Enemy"));
		FSM.actionStates.Grabbing.target = null;
		if (hits.Length > 0) {
			Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
			aimPoint = hits[0].transform.position;
			aimPoint.y += 0.15f;
			FSM.actionStates.Grabbing.target = hits[0].collider.gameObject;
			FSM.actionStates.Yanking.target = hits[0].collider.gameObject;
		}
		Vector3 screenPoint = FSM.cam.WorldToScreenPoint(aimPoint);
		FSM.reticle.position = screenPoint;
	}
	
	public override void OnGrabCommand() {
		FSM.MetaTransition(FSM.metaStates.Normal);
	}
	
	public override void OnYankCommand() {
		FSM.MetaTransition(FSM.metaStates.Normal);
		// currentTarget.GetComponent<EnemyAI>()?.AttemptYank(FSM);
	}
}

public class PlayerAirborneState : PlayerMovementState
{
	private float minimumAirborneTime = 0.05f;
	private float jumpEnterTime;
	
	public PlayerAirborneState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		jumpEnterTime = Time.time;
	}
	
	public override void Tick() {
		base.Tick();
		if (Physics.CheckSphere(FSM.transform.position, 0.1f, LayerMask.GetMask("Ground"))
		//if (FSM.controller.isGrounded
		&& Time.time - jumpEnterTime > minimumAirborneTime) {
			FSM.MovementTransition(FSM.movementStates.Grounded);
			FSM.animator.Play("Land", 1);
		}
	}
	
	public override Command FilterCommand(Command command) {
		if (command is JumpCommand) {
			return new BoundCommand();
		}
		if (command is ChargeRollCommand) {
			return new SlamCommand();
		}
		return command;
	}
	
	public override void OnBoundCommand() {
		Collider[] hits = Physics.OverlapSphere(FSM.transform.position, 3f, LayerMask.GetMask("Enemy"));
		if (hits.Length > 0) {
			Array.Sort(hits, (a, b) => (FSM.transform.position - a.transform.position).magnitude.CompareTo((FSM.transform.position - b.transform.position).magnitude));
		}
		if (hits.Length > 0) {
			int idx = 0;
			while (hits[idx].gameObject.tag == "Boss") {
				idx++;
				if (idx >= hits.Length) return;
			}
			Debug.Log("Bounding");
			FSM.movementStates.Bound.boundedEnemy = hits[idx].GetComponent<EnemyAI>();
			FSM.actionStates.Bounding.boundedEnemy = hits[idx].GetComponent<EnemyAI>();
			FSM.MovementTransition(FSM.movementStates.Bound);
			FSM.ActionTransition(FSM.actionStates.Bounding);
			FSM.movementStates.Bound.boundedEnemy.SetBound(true);
		}
	}
	
	public override void OnSlamCommand() {
		FSM.MovementTransition(FSM.movementStates.Slam);
	}
}

public class PlayerSlamState : PlayerMovementState
{
	private float minimumAirborneTime = 0.05f;
	private float slamEnterTime;
	private bool isSlamming = false;
	
	public PlayerSlamState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		isSlamming = false;
		FSM.currentVelocity.y = 3f;
		slamEnterTime = Time.time;
	}
	
	public override void Exit() {
		isSlamming = false;
	}
	
	public override void Tick() {
		base.Tick();
		Vector3 downAccel = Vector3.zero;
		downAccel.y = FSM.currentVelocity.y;
		FSM.controller.Move(downAccel * 0.5f * Time.deltaTime);
		if (!isSlamming && Physics.CheckSphere(FSM.transform.position, 0.1f, LayerMask.GetMask("Ground"))
			&& Time.time - slamEnterTime > minimumAirborneTime) {
			FSM.animator.Play("Slam");
			isSlamming = true;
		}
	}
	
	public override void OnSlamCommand() {
		if (isSlamming) FSM.movementStates.Grounded.slamQueued = true;
	}
}

public class PlayerBoundState : PlayerMovementState
{
	public EnemyAI boundedEnemy;
	private Vector3 dest;
	private bool atEnemy;
	
	public PlayerBoundState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override Command FilterCommand(Command command) {
		if (!(command is JumpCommand)) {
			return new VoidCommand();
		}
		return command;
	}
	
	public override void Enter() {
		dest = boundedEnemy.transform.position;
		dest.y += boundedEnemy.heightOffset;
		atEnemy = false;
		Physics.IgnoreCollision(FSM.controller, boundedEnemy.GetComponent<Collider>());
	}
	
	public override void Exit() {
		boundedEnemy.SetBound(false);
		Physics.IgnoreCollision(FSM.controller, boundedEnemy.GetComponent<Collider>(), false);
		boundedEnemy = null;
	}
	
	public override void Tick() {
		if (atEnemy) return;
		Vector3 inputDir = dest - FSM.transform.position;
		inputDir = inputDir.magnitude > Time.deltaTime * 5f ? inputDir.normalized * Time.deltaTime * 5f: inputDir;
		if (inputDir == dest - FSM.transform.position) atEnemy = true;
		FSM.currentVelocity = inputDir;
		FSM.controller.Move(FSM.currentVelocity);
		inputDir.y = 0f;
		FSM.transform.rotation = Quaternion.LookRotation(inputDir);
	}
	
	public override void OnJumpCommand() {
		if (!atEnemy) return;
		FSM.MovementTransition(FSM.movementStates.Grounded);
		FSM.ActionTransition(FSM.actionStates.Idle);
		FSM.movementStates.Grounded.OnJumpCommand();
	}
}

public class PlayerStunState : PlayerMovementState
{
	public float stunTimer;
	public Vector3 knockbackDir;
	
	public PlayerStunState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.currentVelocity = knockbackDir;
	}
	
	public override void Tick() {
		stunTimer -= Time.deltaTime;
		if (stunTimer <= 0f) {
			stunTimer = 0f;
			FSM.MovementTransition(FSM.movementStates.Grounded);
		} else {
			if (knockbackDir != Vector3.zero) {
				Vector3 currentVelocity = knockbackDir;
				if (currentVelocity.y > Physics.gravity.y) currentVelocity.y += Physics.gravity.y * Time.deltaTime;
				FSM.currentVelocity = currentVelocity;
				FSM.controller.Move(knockbackDir * Time.deltaTime);
			}
		}
	}
}

public class PlayerIdleState : PlayerActionState
{
	public PlayerActionState actionQueue;
	
	public PlayerIdleState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void OnJabCommand() {
		FSM.ActionTransition(FSM.actionStates.Jabbing);
	}
	
	public override void OnAttenuateCommand() {
		FSM.ActionTransition(FSM.actionStates.Attenuate);
	}
	
	public override void OnConsumeCommand() {
		FSM.ActionTransition(FSM.actionStates.Consuming);
	}
	
	public override void OnChargeRollCommand() {
		FSM.ActionTransition(FSM.actionStates.ChargeRoll);
	}
	
	public override void OnSlamCommand() {
		FSM.ActionTransition(FSM.actionStates.Slamming);
	}
	
	public override void OnSpineCommand() {
		GameObject spine = Transform.Instantiate(FSM.attackProfile.spineProjectile, FSM.transform.position + new Vector3(0f, 0.2f, 0f), Quaternion.identity);
		Collider[] hits = Physics.OverlapSphere(FSM.transform.position, 12f, LayerMask.GetMask("Enemy"));
		if (hits.Length >= 1) {
			Vector3 enemyDir = hits[0].transform.position - FSM.transform.position;
			Debug.Log("Enemy at " + enemyDir);
			spine.transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.forward, enemyDir), enemyDir);
		}
	}
	
	public override void OnImpaleCommand() {
		FSM.ActionTransition(FSM.actionStates.Impaling);
	}
	
	public override void OnGrabCommand() {
		FSM.ActionTransition(FSM.actionStates.Grabbing);
	}
	
	public override void OnYankCommand() {
		FSM.ActionTransition(FSM.actionStates.Yanking);
	}
	
	public override void Tick() {
		if (actionQueue != null) {
			FSM.ActionTransition(actionQueue);
			actionQueue = null;
		}
	}
}

public class PlayerJabbingState : PlayerActionState
{
	// public float jabEnterTime { get; private set; }
	private IAttackDelegate curAttack;
	private IAttackDelegate[] attackSets;
	private int qmode = 0;

	public PlayerJabbingState(PlayerStateMachine ctx) : base(ctx) {
		attackSets = new IAttackDelegate[4] { new BasicChainDelegate(), 
			new LargeChainDelegate(), 
			new PierceChainDelegate(),
			new SlashChainDelegate() };
		curAttack = attackSets[0];
	}
	
	public override void ModeSet(int mode) {
		qmode = mode;
	}
	
	public override void Enter() {
		// Snap to nearest enemy
		Collider[] hits = Physics.OverlapSphere(FSM.transform.position, 1.6f, LayerMask.GetMask("Enemy"));
		if (hits.Length >= 1) {
			Vector3 enemyDir = hits[0].transform.position - FSM.transform.position;
			enemyDir.y = 0;
			FSM.transform.rotation = Quaternion.LookRotation(enemyDir);
			FSM.controller.Move(enemyDir * Mathf.Max(0, enemyDir.magnitude - 1.3f));
		}
		// jabEnterTime = Time.time;
		curAttack = attackSets[qmode];
		if (FSM.equippedOffense == 2) curAttack = attackSets[2];
		if (FSM.equippedOffense == 3) curAttack = attackSets[3];
		FSM.movementAllowed = false;
		curAttack.Execute(FSM);
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}

	public override void OnJabCommand() {
		FSM.actionStates.Idle.actionQueue = FSM.actionStates.Jabbing;
	}
	
	public override void OnBoundCommand() {
		if (FSM.actionStates.Bounding.boundedEnemy != null) {
			FSM.animationCancelled = true;
		}
	}
	
	public override void OnAttenuateCommand() {
		FSM.animationCancelled = true;
		FSM.ActionTransition(FSM.actionStates.Attenuate);
	}
	
	public override void OnConsumeCommand() {
		FSM.animationCancelled = true;
		FSM.ActionTransition(FSM.actionStates.Consuming);
	}
}

public class PlayerAttenuateState : PlayerActionState
{
	public float lastExit;
	
	public PlayerAttenuateState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
		FSM.animator.Play("Attenuate");
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
		lastExit = Time.time;
	}
	
	public override Command FilterCommand(Command command) {
		if (command is JumpCommand || command is BoundCommand) {
			return new VoidCommand();
		}
		return command;
	}
	
	public override void OnConsumeCommand() {
		FSM.animationCancelled = true;
		FSM.ActionTransition(FSM.actionStates.Consuming);
	}
}

public class PlayerBoundingState : PlayerActionState
{
	public EnemyAI boundedEnemy;
	
	public PlayerBoundingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
		FSM.animator.Play("EmptyAnimanim");
	}

	public override void Exit() {
		boundedEnemy = null;
		FSM.movementAllowed = true;
	}
}

public class PlayerConsumingState : PlayerActionState
{
	public PlayerConsumingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
		FSM.animator.Play("Consume");
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}
	
	public override Command FilterCommand(Command command) {
		if (command is JumpCommand || command is BoundCommand) {
			return new VoidCommand();
		}
		return command;
	}
	
	public override void OnAttenuateCommand() {
		FSM.animationCancelled = true;
		FSM.ActionTransition(FSM.actionStates.Attenuate);
	}
}

public class PlayerChargeRollState : PlayerActionState
{
	private float chargeTime;
	
	public PlayerChargeRollState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
		FSM.animator.Play("ChargeRoll", 2);
		chargeTime = 0f;
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
		FSM.actionStates.Rolling.power = Mathf.Min(chargeTime, 5f);
	}
	
	public override void Tick() {
		chargeTime += Time.deltaTime;
	}
}

public class PlayerRollingState : PlayerActionState
{
	public float power;
	private float pingBuffer;
	private bool pingedFrom = false;
	public GameObject lastHit;
	public float lastHitTime;
	
	public PlayerRollingState(PlayerStateMachine ctx) : base(ctx) {	}
	
	public override void Enter() {
		pingBuffer = Time.time - 3f;
		FSM.movementAllowed = false;
		// TODO: Make it directionally biased (favoured to target enemies in stick direction)
		Collider[] hits = Physics.OverlapSphere(FSM.transform.position, 12f, LayerMask.GetMask("Enemy"));
		if (hits.Length >= 2 || (hits.Length == 1 && hits[0].gameObject != lastHit)) {
			int idx = 0;
			if (hits[idx].gameObject == lastHit) idx = 1;
			Vector3 enemyDir = hits[idx].transform.position - FSM.transform.position;
			enemyDir.y = 0;
			FSM.transform.rotation = Quaternion.LookRotation(enemyDir);
		} else {
			FSM.transform.rotation = FSM.transform.rotation * Quaternion.Euler(0, 180, 0);
		}
		if (!pingedFrom) FSM.animator.Play("Pre-Roll", 1);
		pingedFrom = false;
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
		if (FSM.actionStates.Idle.actionQueue == FSM.actionStates.Rolling) {
			power += 0.5f;
			FSM.animator.Play("Pingtest", 1);
			pingedFrom = true;
		} else {
			FSM.animator.Play("Unroll", 1);
		}
	}
	
	public override void Tick() {
		// Process Pings and Enemy contact I guess
		FSM.controller.Move(FSM.transform.forward * power * 5f * Time.deltaTime);
		// After transition to Idle, check if within buffer
		// Only allow if transition from contact, that is clear the pingBuffer from other transition sources
		if (Time.time - pingBuffer < 3f) {
			FSM.actionStates.Idle.actionQueue = FSM.actionStates.Rolling;
		} else {
			FSM.actionStates.Idle.actionQueue = null;
		}
		if (lastHit != null && Time.time - lastHitTime >= 0.3f) {
			lastHit = null;
		}
	}

	public override void OnJabCommand() {
		pingBuffer = Time.time;
	}
	
	public override Command FilterCommand(Command command) {
		if (command is JumpCommand) {
			return new VoidCommand();
		}
		return command;
	}
}

public class PlayerSlammingState : PlayerActionState
{
	private float pingBuffer;
	
	public PlayerSlammingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		pingBuffer = Time.time - 1.5f;
		//FSM.movementAllowed = false;
		Physics.IgnoreLayerCollision(3, 7, true);
	}

	public override void Exit() {
		Physics.IgnoreLayerCollision(3, 7, false);
		//FSM.movementAllowed = true;
	}

	public override void Tick() {
		// After transition to Idle, check if within buffer
		// Only allow if transition from contact, that is clear the pingBuffer from other transition sources
		if (Time.time - pingBuffer < 1.5f) {
			FSM.actionStates.Idle.actionQueue = FSM.actionStates.Rolling;
			FSM.actionStates.Rolling.power = 1.9f;
		} else {
			FSM.actionStates.Idle.actionQueue = null;
		}
	}

	public override void OnJabCommand() {
		pingBuffer = Time.time;
	}
}

public class PlayerImpalingState : PlayerActionState
{
	private float impaleCharge;
	
	public PlayerImpalingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		impaleCharge = Time.time;
	}
	
	public override void Exit() {
		if (Time.time - impaleCharge >= 0.5f) {
			Collider[] hits = Physics.OverlapSphere(FSM.transform.position, 10f, LayerMask.GetMask("Enemy"));			
			for (int i = 0; i < hits.Length; i++) {
				GameObject impaleSeeker = Transform.Instantiate(FSM.attackProfile.impaleSeeker, FSM.transform.position, Quaternion.identity);
				impaleSeeker.GetComponent<ImpaleSeeker>().target = hits[i].gameObject;
			}
		}
	}
}

public class PlayerGrabbingState : PlayerActionState
{
	public GameObject target;
	
	public PlayerGrabbingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
		if (target != null) {
			FSM.transform.rotation = Quaternion.LookRotation(target.transform.position - FSM.transform.position);
		}
		FSM.animator.Play("Grab", 2);
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}
}

public class PlayerYankingState : PlayerActionState
{
	public GameObject target;
	
	public PlayerYankingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
		if (target != null) {
			FSM.transform.rotation = Quaternion.LookRotation(target.transform.position - FSM.transform.position);
		}
		FSM.animator.Play("Yank", 2);
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}
}

public class PlayerStunnedState : PlayerActionState
{
	public float stunTimer;
	
	public PlayerStunnedState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.movementAllowed = false;
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}
	
	public override void Tick() {
		stunTimer -= Time.deltaTime;
		if (stunTimer <= 0f) {
			stunTimer = 0f;
			FSM.ActionTransition(FSM.actionStates.Idle);
		}
	}
}

public class PlayerNormalState : PlayerMetaState
{
	public PlayerNormalState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.actionStates.Jabbing.ModeSet(0);
	}
}

public class PlayerCurledState : PlayerMetaState
{
	public PlayerCurledState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.animator.Play("Curl", 0);
		FSM.actionStates.Jabbing.ModeSet(1);
	}
	
	public override void Exit() {
		FSM.movementStates.Grounded.slamQueued = false;
		FSM.actionStates.Idle.actionQueue = null;
		FSM.MovementTransition(FSM.movementStates.Grounded);
		// Handle Scenario when Jabs are interrupted
		// Either stop the Jab (Cancel the animation and let the state machine rider resolve)
		// Or allow the jab to follow through and stall the transition
		// The latter probably requires some re-factoring to allow for selective canceling of Roll/Slam
		// Plus clarification on Attenuate and other gear wheel stuff
		//// Right now, just force-cancel everything
		FSM.animator.Play("EmptyAnimanim", 2);
		FSM.animator.Play("Idle", 1);
		FSM.ActionTransition(FSM.actionStates.Idle);
		// For some reason the Unity animator sucks with scale so I'm fixing it here
		FSM.animator.Play("Uncurl", 0);
	}
	
	public override Command FilterCommand(Command command) {
		if (command is ConsumeCommand) {
			return new ChargeRollCommand();
		}
		return command;
	}
}

public class PlayerLockdownState : PlayerMetaState
{
	public PlayerLockdownState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.animator.Play("Lockdown", 0);
		FSM.ActionTransition(FSM.actionStates.Idle);
		FSM.movementAllowed = false;
	}
	
	public override void Exit() {
		FSM.movementStates.Grounded.slamQueued = false;
		FSM.actionStates.Idle.actionQueue = null;
		FSM.MovementTransition(FSM.movementStates.Grounded);
		FSM.animator.Play("EmptyAnimanim", 2);
		FSM.ActionTransition(FSM.actionStates.Idle);
		FSM.movementAllowed = true;
		FSM.animator.Play("Un-Lockdown", 0);
	}
	
	public override Command FilterCommand(Command command) {
		if (command is JabCommand) {
			return new SpineCommand();
		}
		if (command is ConsumeCommand) {
			return new ImpaleCommand();
		}
		return new VoidCommand();
	}
}

public class PlayerAimState : PlayerMetaState
{
	public PlayerAimState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.MovementTransition(FSM.movementStates.Aiming);
		FSM.SetCam(1);
		FSM.reticle.gameObject.SetActive(true);
	}
	
	public override void Exit() {
		FSM.SetCam(0);
		FSM.reticle.gameObject.SetActive(false);
		FSM.movementStates.Grounded.slamQueued = false;
		FSM.actionStates.Idle.actionQueue = null;
		FSM.MovementTransition(FSM.movementStates.Grounded);
		FSM.animator.Play("EmptyAnimanim", 2);
		FSM.ActionTransition(FSM.actionStates.Idle);
	}
	
	public override Command FilterCommand(Command command) {
		if (command is JabCommand) {
			return new GrabCommand();
		}
		if (command is ConsumeCommand) {
			return new YankCommand();
		}
		return new VoidCommand();
	}
}