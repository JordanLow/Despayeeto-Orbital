using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
	// Implementing a Hierarchal Finite State Machine with Orthogonal Regions
	// Initializing the States
	
	// Regions and States for this State Machine
	public class MovementRegion {
		public PlayerGroundedState Grounded { get; }
		public PlayerAirborneState Airborne { get; }
		
		public MovementRegion(PlayerGroundedState groundedState, 
			PlayerAirborneState airborneState) {
			
			this.Grounded = groundedState;
			this.Airborne = airborneState;
		}
	}
	
	public class ActionRegion {
		public PlayerIdleState Idle { get; }
		public PlayerJabbingState Jabbing { get; }
		
		public ActionRegion(PlayerIdleState idleState,
			PlayerJabbingState jabbingState) {
			
			this.Idle = idleState;
			this.Jabbing = jabbingState;
		}
	}
	
	public MovementRegion movementStates { get; private set; }
	public ActionRegion actionStates { get; private set; }
	private PlayerMovementState currentMovement;
	private PlayerActionState currentAction;

    // Update is called once per frame
    void Update()
    {
        this.currentMovement?.Tick();
		this.currentAction?.Tick();
    }
	
	public void MovementTransition(PlayerMovementState newState) {
		this.currentMovement?.Exit();
		this.currentMovement = newState;
		this.currentMovement?.Enter();
		Debug.Log("Transiting to " + this.currentMovement);
	}
	
	public void ActionTransition(PlayerActionState newState) {
		this.currentAction?.Exit();
		this.currentAction = newState;
		this.currentAction?.Enter();
		Debug.Log("Transiting to " + this.currentAction);
	}
	
	// Input Control
	public Vector3 inputDirection;
	
	public void OnMove(InputValue val) {
		Vector2 direction = val.Get<Vector2>() * moveSpeed;
		this.inputDirection.x = direction.x;
		this.inputDirection.z = direction.y;
	}
	
	public void OnJump() {
		Command command = new JumpCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnJab() {
		Command command = new JabCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	// Player Values
	
	public Camera cam { get; private set; }
	public CharacterController controller { get; private set; }
	public Vector3 currentVelocity;
	
	[SerializeField] public float moveSpeed { get; } = 8f;
	[SerializeField] public float jumpSpeed { get; } = 4f;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.movementStates = new MovementRegion(
			new PlayerGroundedState(this),
			new PlayerAirborneState(this)
		);
		this.actionStates = new ActionRegion(
			new PlayerIdleState(this),
			new PlayerJabbingState(this)
		);
		this.inputDirection = Vector3.zero;
		this.currentVelocity = Vector3.zero;
		this.cam = Camera.main;
		this.controller = GetComponent<CharacterController>();
		this.MovementTransition(movementStates.Grounded);
		this.ActionTransition(actionStates.Idle);
    }
}
