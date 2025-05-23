using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

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
		public PlayerAttenuateState Attenuate { get; }
		
		public ActionRegion(PlayerIdleState idleState,
			PlayerJabbingState jabbingState,
			PlayerAttenuateState attenuateState) {
			
			this.Idle = idleState;
			this.Jabbing = jabbingState;
			this.Attenuate = attenuateState;
		}
	}
	
	public MovementRegion movementStates { get; private set; }
	public ActionRegion actionStates { get; private set; }
	private PlayerMovementState currentMovement;
	private PlayerActionState currentAction;

    // Update is called once per frame
    void Update()
    {
		this.currentAction?.Tick();
        this.currentMovement?.Tick();
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
	public bool movementAllowed = true;
	[SerializeField] GameObject offenseGearWheel;
	
	public void OnMove(InputAction.CallbackContext context) {
		Vector2 direction = context.ReadValue<Vector2>() * moveSpeed;
		this.inputDirection.x = direction.x;
		this.inputDirection.z = direction.y;
	}
	
	public void OnJump() {
		Command command = new JumpCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnJab(InputAction.CallbackContext context) {
		if (!context.started) return;
		Command command = new JabCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnAttenuate() {
		Command command = new AttenuateCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnGearWheel(InputAction.CallbackContext context) { //Just for offense for now
		if (context.started) {
			Time.timeScale = 0.3f;
			// Pop up HUD
			offenseGearWheel.SetActive(true);
			// Disable camera look
			CinemachineCamera cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
			cam.GetComponent<CinemachineInputAxisController>().enabled = false;
		} else if (context.canceled) {
			Time.timeScale = 1f;
			// Close HUD and queue weapon swap
			offenseGearWheel.SetActive(false);
			// Re-enable camera look
			CinemachineCamera cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
			cam.GetComponent<CinemachineInputAxisController>().enabled = true;
			//Update Weapon
			Debug.Log("Mode: " + offenseGearWheel.GetComponent<RadialMenu>().att);
			actionStates.Jabbing.modeSet(offenseGearWheel.GetComponent<RadialMenu>().att);
		}
	}
	
	// Player Values
	
	public Camera cam { get; private set; }
	public CharacterController controller { get; private set; }
	public Animator animator { get; private set; }
	public Vector3 currentVelocity;
	
	[SerializeField] public float moveSpeed { get; } = 8f;
	[SerializeField] public float jumpSpeed { get; } = 4f;
	[SerializeField] public PlayerAttackSet attackProfile;
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.movementStates = new MovementRegion(
			new PlayerGroundedState(this),
			new PlayerAirborneState(this)
		);
		this.actionStates = new ActionRegion(
			new PlayerIdleState(this),
			new PlayerJabbingState(this),
			new PlayerAttenuateState(this)
		);
		this.inputDirection = Vector3.zero;
		this.currentVelocity = Vector3.zero;
		this.cam = Camera.main;
		this.controller = GetComponent<CharacterController>();
		this.animator = GetComponent<Animator>();
		this.MovementTransition(movementStates.Grounded);
		this.ActionTransition(actionStates.Idle);
    }
}
