using UnityEngine;

public interface PlayerState
{
    public abstract void Enter();
	public abstract void Exit();
	public abstract void Tick();
}

public class PlayerBaseState : PlayerState
{
	private readonly PlayerStateMachine FSM;
	
	public PlayerBaseState(PlayerStateMachine ctx) {
		this.FSM = ctx;
	}
	
	public virtual void Enter() {
		FSM.jumpListener += Jump;
		FSM.jabListener += Jab;
	}
	
	public virtual void Exit() {
		FSM.jumpListener -= Jump;
		FSM.jabListener -= Jab;
	}
	
	public virtual void Tick() {
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
	
	private void Jump() {
		FSM.currentVelocity.y = FSM.jumpSpeed;
	}
	
	private void Jab() {
		
	}
}

public class PlayerAirborneState : PlayerState
{
	private readonly PlayerStateMachine FSM;
	
	public PlayerAirborneState(PlayerStateMachine ctx) {
		this.FSM = ctx;
	}
	
	public void Enter() {
		FSM.jabListener += Jab;
	}
	
	public void Exit() {
		FSM.jabListener -= Jab;
	}
	
	public void Tick() {
		// If on ground switch back to base
	}
	
	public void Jab() {
		
	}
}

public class PlayerJabbingState : PlayerBaseState
{
	private readonly PlayerStateMachine FSM;
	
	public PlayerJabbingState(PlayerStateMachine ctx) : base(ctx) {
		this.FSM = ctx;
	}
	
	public override void Enter() {
		FSM.jumpListener += Jump;
		FSM.jabListener += Jab;
	}
	
	public override void Exit() {
		FSM.jumpListener -= Jump;
		FSM.jabListener -= Jab;
	}
	
	public override void Tick() {
		
	}
	
	private void Jump() {
		
	}
	
	private void Jab() {
		
	}
}

