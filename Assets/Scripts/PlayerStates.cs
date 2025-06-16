using UnityEngine;

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
	
	public virtual void modeSet(int mode) { }
}

public abstract class PlayerMetaState : PlayerState
{
	protected readonly PlayerStateMachine FSM;
	
	public PlayerMetaState(PlayerStateMachine ctx) {
		this.FSM = ctx;
	}
	
	public virtual void modeSet(int mode) { }
}

public class PlayerGroundedState : PlayerMovementState
{
	public PlayerGroundedState(PlayerStateMachine ctx) : base(ctx) { }
	private bool isJumping = false;
	public bool slamQueued = false;

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
			isJumping = false;
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
		FSM.animator.Play("BounceSquish");
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
			FSM.animator.Play("Land");
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
			Debug.Log("Bounding");
			FSM.movementStates.Bound.boundedEnemy = hits[0].GetComponent<EnemyAI>();
			FSM.actionStates.Bounding.boundedEnemy = hits[0].GetComponent<EnemyAI>();
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
	
	public override void Tick() {
		if (actionQueue != null) {
			FSM.ActionTransition(actionQueue);
			actionQueue = null;
		}
	}
}

public class PlayerJabbingState : PlayerActionState
{
	public float jabEnterTime { get; private set; }
	private IAttackDelegate curAttack;
	private IAttackDelegate[] attackSets;
	private int qmode = 0;

	public PlayerJabbingState(PlayerStateMachine ctx) : base(ctx) {
		attackSets = new IAttackDelegate[1] { new BasicChainDelegate() };
		curAttack = attackSets[0];
	}
	
	public override void modeSet(int mode) {
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
		jabEnterTime = Time.time;
		curAttack = attackSets[qmode];
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
		FSM.animator.Play("ChargeRoll", 1);
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
	
	public PlayerRollingState(PlayerStateMachine ctx) : base(ctx) {	}
	
	public override void Enter() {
		FSM.movementAllowed = false;
		FSM.animator.Play("Roll", 1);
		// Lock onto some nearby enemy, probably with a dynamically updating direction
	}
	
	public override void Exit() {
		FSM.animator.Play("EmptyAnimanim", 1);
		FSM.movementAllowed = true;
	}
	
	public override void Tick() {
		// Process Pings and Enemy contact I guess
		FSM.controller.Move(FSM.transform.forward * power * 5f * Time.deltaTime);
	}
	
	public override void OnJumpCommand() {
		FSM.ActionTransition(FSM.actionStates.Idle);
	}
}

public class PlayerSlammingState : PlayerActionState
{
	public PlayerSlammingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		//FSM.movementAllowed = false;
		Physics.IgnoreLayerCollision(3, 7, true);
	}
	
	public override void Exit() {
		Physics.IgnoreLayerCollision(3, 7, false);
		//FSM.movementAllowed = true;
	}
}

public class PlayerNormalState : PlayerMetaState
{
	public PlayerNormalState(PlayerStateMachine ctx) : base(ctx) { }
}

public class PlayerCurledState : PlayerMetaState
{
	public PlayerCurledState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		FSM.animator.Play("Curl", 2);
	}
	
	public override void Exit() {
		FSM.movementStates.Grounded.slamQueued = false;
		FSM.MovementTransition(FSM.movementStates.Grounded);
		FSM.ActionTransition(FSM.actionStates.Idle);
		FSM.animator.Play("Uncurl", 2);
	}
	
	public override Command FilterCommand(Command command) {
		if (command is ConsumeCommand) {
			return new ChargeRollCommand();
		}
		return command;
	}
}