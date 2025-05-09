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
}

public class PlayerGroundedState : PlayerMovementState
{
	public PlayerGroundedState(PlayerStateMachine ctx) : base(ctx) { }

	public override void OnJumpCommand() {
		FSM.currentVelocity.y = FSM.jumpSpeed;
		// Transition to Jump
	}
}

public class PlayerAirborneState : PlayerMovementState
{
	public PlayerAirborneState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Tick() {
		base.Tick();
		// Check if Grounded and Transition if so
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
	public PlayerJabbingState(PlayerStateMachine ctx) : base(ctx) { }
	
	public override void Enter() {
		Debug.Log("Pretend I jabbed");
	}
	
	public override void Exit() {
		Debug.Log("Now I'm done jabbing");
	}
	
	public override void Tick() {
		FSM.ActionTransition(FSM.actionStates.Idle);
	}
}

