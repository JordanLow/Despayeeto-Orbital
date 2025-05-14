using UnityEngine;

public abstract class PlayerState
{
    public virtual void Enter() { }
	public virtual void Exit() { }
	public virtual void Tick() { }
	
	public virtual Command FilterCommand(Command command) { return command; }
	
	public virtual void OnJumpCommand() { }
	public virtual void OnJabCommand() { }
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
		Vector3 inputDirection = FSM.inputDirection;
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
	
	public override void Enter() {
		
	}

	public override void Tick() {
		base.Tick();
		if (!Physics.CheckSphere(FSM.transform.position, 0.1f, LayerMask.GetMask("Ground"))) {
		//if (!FSM.controller.isGrounded) {
			FSM.MovementTransition(FSM.movementStates.Airborne);
		}
	}

	public override void OnJumpCommand() {
		FSM.currentVelocity.y = FSM.jumpSpeed;
		//FSM.MovementTransition(FSM.movementStates.Airborne);
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
		}
	}
}

public class PlayerIdleState : PlayerActionState
{
	public PlayerIdleState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void OnJabCommand() {
		FSM.ActionTransition(FSM.actionStates.Jabbing);
	}
}

public class PlayerJabbingState : PlayerActionState
{
	public float jabEnterTime { get; private set; }
	private bool followQueued = false;
	private IAttackDelegate curAttack;
	private IAttackDelegate[] attackSets;

	public PlayerJabbingState(PlayerStateMachine ctx) : base(ctx) {
		attackSets = new IAttackDelegate[2] { new RapidsDelegate(), new GentlemanDelegate() };
		curAttack = attackSets[0];
	}
	
	public override void modeSet(int mode) {
		Debug.Log("Switching to mode " + mode);
		curAttack = attackSets[mode];
	}
	
	public override void Enter() {
		jabEnterTime = Time.time;
		followQueued = false;
		curAttack.Execute(FSM);
	}

	public override Command FilterCommand(Command command) { 
		// Decision tree pending refactor for elegance based on
		// Representation of attacksets
		// Right now lists and numbers are used for ease
		if (curAttack == attackSets[1] && command is JumpCommand) {
			return new VoidCommand();
		}
		return command; 
	}
	
	public override void Tick() {
		if (curAttack.isFinished) {
			FSM.ActionTransition(FSM.actionStates.Idle);
		}
	}

	//public override OnJabCommand() {
		
	//}
}

