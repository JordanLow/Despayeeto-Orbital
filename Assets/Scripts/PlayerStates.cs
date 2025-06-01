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

public class PlayerGroundedState : PlayerMovementState
{
	public PlayerGroundedState(PlayerStateMachine ctx) : base(ctx) { }
	private bool isJumping = false;

	public override void Tick() {
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
		return command;
	}

	public override void OnJumpCommand() {
		isJumping = true;
		FSM.animator.Play("BounceSquish");
	}
}

public class PlayerAirborneState : PlayerMovementState
{
	public PlayerAirborneState(PlayerStateMachine ctx) : base(ctx) { }
	private float minimumAirborneTime = 0.05f;
	private float jumpEnterTime;
	
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
		return command;
	}
	
	public override void OnBoundCommand() {
		Collider[] hits = Physics.OverlapSphere(FSM.transform.position, 3f, LayerMask.GetMask("Enemy"));
		if (hits.Length > 0) {
			FSM.movementStates.Bound.boundedEnemy = hits[0].GetComponent<EnemyAI>();
			FSM.actionStates.Bounding.boundedEnemy = hits[0].GetComponent<EnemyAI>();
			FSM.MovementTransition(FSM.movementStates.Bound);
			FSM.movementStates.Bound.boundedEnemy.SetBound(true);
		}
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
		boundedEnemy.SetBound(false);
		Physics.IgnoreCollision(FSM.controller, boundedEnemy.GetComponent<Collider>(), false);
		FSM.movementStates.Grounded.OnJumpCommand();
	}
}

public class PlayerIdleState : PlayerActionState
{
	public bool jabBuffer = false;
	
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
	
	public override void OnBoundCommand() {
		FSM.ActionTransition(FSM.actionStates.Bounding);
	}
	
	public override void Tick() {
		if (jabBuffer) {
			jabBuffer = false;
			FSM.ActionTransition(FSM.actionStates.Jabbing);
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
		attackSets = new IAttackDelegate[2] { new BasicChainDelegate(), new GentlemanDelegate() };
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
		FSM.actionStates.Idle.jabBuffer = false;
		curAttack = attackSets[qmode];
		FSM.movementAllowed = false;
		curAttack.Execute(FSM);
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}

	public override void OnJabCommand() {
		FSM.actionStates.Idle.jabBuffer = true;
	}
	
	public override void OnBoundCommand() {
		FSM.animationCancelled = true;
		FSM.ActionTransition(FSM.actionStates.Bounding);
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