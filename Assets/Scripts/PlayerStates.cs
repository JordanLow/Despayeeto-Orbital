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

	public override void OnJumpCommand() {
		if (isJumping) return;
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
			return new VoidCommand();
		}
		return command;
	}
}

public class PlayerIdleState : PlayerActionState
{
	public bool jabBuffer = false;
	
	public PlayerIdleState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		Debug.Log("Jab Buffer: " + jabBuffer);
	}
	
	public override void OnJabCommand() {
		FSM.ActionTransition(FSM.actionStates.Jabbing);
	}
	
	public override void OnAttenuateCommand() {
		FSM.ActionTransition(FSM.actionStates.Attenuate);
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
		curAttack.Execute(FSM);
	}

	public override void OnJabCommand() {
		Debug.Log("Buffering Jab");
		FSM.actionStates.Idle.jabBuffer = true;
	}

	public override void OnJumpCommand() {
		FSM.actionStates.Idle.jabBuffer = false;
	}
}

public class PlayerAttenuateState : PlayerActionState
{
	public float attenuateEnterTime;
	
	public PlayerAttenuateState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		attenuateEnterTime = Time.time;
		FSM.movementAllowed = false;
	}
	
	public override void Exit() {
		FSM.movementAllowed = true;
	}
	
	public override void Tick() {
		// Also tie this to AnimationEvents
		if (Time.time - attenuateEnterTime >= 1f) {
			FSM.ActionTransition(FSM.actionStates.Idle);
		}
	}
}