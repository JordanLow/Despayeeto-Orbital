using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;

public class PlayerStateMachine : MonoBehaviour
{
	// Implementing a Hierarchal Finite State Machine with Orthogonal Regions
	// Initializing the States
	
	// Regions and States for this State Machine
	public class MovementRegion {
		public PlayerGroundedState Grounded { get; }
		public PlayerAirborneState Airborne { get; }
		public PlayerBoundState Bound { get; }
		
		public MovementRegion(PlayerGroundedState groundedState, 
			PlayerAirborneState airborneState,
			PlayerBoundState boundState) {
			
			this.Grounded = groundedState;
			this.Airborne = airborneState;
			this.Bound = boundState;
		}
	}
	
	public class ActionRegion {
		public PlayerIdleState Idle { get; }
		public PlayerJabbingState Jabbing { get; }
		public PlayerAttenuateState Attenuate { get; }
		public PlayerBoundingState Bounding { get; }
		public PlayerConsumingState Consuming { get; }
		
		public ActionRegion(PlayerIdleState idleState,
			PlayerJabbingState jabbingState,
			PlayerAttenuateState attenuateState,
			PlayerBoundingState boundingState,
			PlayerConsumingState consumingState) {
			
			this.Idle = idleState;
			this.Jabbing = jabbingState;
			this.Attenuate = attenuateState;
			this.Bounding = boundingState;
			this.Consuming = consumingState;
		}
	}
	
	public MovementRegion movementStates { get; private set; }
	public ActionRegion actionStates { get; private set; }
	private PlayerMovementState currentMovement;
	private PlayerActionState currentAction;

    // Update is called once per frame
    void Update()
    {
		if (HP <= 0) {
			hpFillImg.rectTransform.localScale = new Vector3(0, 1, 1);
			return;
		} else {
			hpFillImg.rectTransform.localScale = new Vector3(Mathf.Clamp01(HP / maxHP), 1, 1);
		}
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
	
	public void OnJump(InputAction.CallbackContext context) {
		if (!context.started) return;
		Command command = new JumpCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	public void JumpHelper() {
		currentVelocity.y = jumpSpeed;
	}
	
	public void OnJab(InputAction.CallbackContext context) {
		if (!context.started) return;
		Command command = new JabCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnAttenuate(InputAction.CallbackContext context) {
		if (!context.started) return;
		if (Time.time - actionStates.Attenuate.lastExit <= 2f) return;
		Command command = new AttenuateCommand();
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnConsume(InputAction.CallbackContext context) {
		if (!context.started) return;
		Command command = new ConsumeCommand();
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
	
	public bool animationCancelled = false;
	
	[SerializeField] public float moveSpeed = 8f;
	[SerializeField] public float jumpSpeed = 4f;
	[SerializeField] public PlayerAttackSet attackProfile;
	[SerializeField] private Image hpFillImg;
	
	[SerializeField] private float maxHP = 50f;
	[SerializeField] private float healAmt = 50f;
	private float HP;
	
	// Maybe move this stuff in a separate script if it gets too clunky
	
	public void getHit(GameObject src, float dmg) {
		if (currentAction == actionStates.Attenuate) return;
		HP -= dmg;
		if (HP <= 0) {
			animator.Play("Deflate");
			GetComponent<PlayerInput>().DeactivateInput();
		}
	}
	public void BasicHeal() {
		HP += healAmt;
	}
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.movementStates = new MovementRegion(
			new PlayerGroundedState(this),
			new PlayerAirborneState(this),
			new PlayerBoundState(this)
		);
		this.actionStates = new ActionRegion(
			new PlayerIdleState(this),
			new PlayerJabbingState(this),
			new PlayerAttenuateState(this),
			new PlayerBoundingState(this),
			new PlayerConsumingState(this)
		);
		this.inputDirection = Vector3.zero;
		this.currentVelocity = Vector3.zero;
		this.cam = Camera.main;
		this.controller = GetComponent<CharacterController>();
		this.animator = GetComponent<Animator>();
		this.MovementTransition(movementStates.Grounded);
		this.ActionTransition(actionStates.Idle);
		HP += maxHP;
		
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
    }
}
