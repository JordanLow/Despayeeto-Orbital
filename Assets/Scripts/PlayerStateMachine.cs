using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
	// Initializing the States
	
	// List of States for this State Machine
	public class States {
		public PlayerBaseState Base { get; }
		public PlayerAirborneState Airborne { get; }
		public PlayerJabbingState Jabbing { get; }
		
		public States(PlayerBaseState baseState, 
			PlayerAirborneState airborneState, 
			PlayerJabbingState jabbingState) {
			
			this.Base = baseState;
			this.Airborne = airborneState;
			this.Jabbing = jabbingState;
		}
	}
	
	public States states { get; private set; }
	private PlayerState currentState;

    // Update is called once per frame
    void Update()
    {
        this.currentState?.Tick();
    }
	
	public void Transition(PlayerState newState) {
		this.currentState?.Exit();
		this.currentState = newState;
		this.currentState?.Enter();
	}
	
	// Input Control Listeners
	
	public Action jumpListener;
	public Action jabListener;
	public Vector3 inputDirection;
	
	public void OnMove(InputValue val) {
		Vector2 direction = val.Get<Vector2>() * moveSpeed;
		this.inputDirection.x = direction.x;
		this.inputDirection.z = direction.y;
	}
	
	public void OnJump() {
		jumpListener?.Invoke();
	}
	
	public void OnJab() {
		jabListener?.Invoke();
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
        this.states = new States(
			new PlayerBaseState(this),
			new PlayerAirborneState(this),
			new PlayerJabbingState(this)
		);
		this.inputDirection = Vector3.zero;
		this.currentVelocity = Vector3.zero;
		this.cam = Camera.main;
		this.controller = GetComponent<CharacterController>();
		this.Transition(states.Base);
    }
}
