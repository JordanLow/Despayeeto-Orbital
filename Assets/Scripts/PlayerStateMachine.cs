using System;
using System.Collections.Generic;
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
		public PlayerAimingState Aiming { get; }
		public PlayerAirborneState Airborne { get; }
		public PlayerBoundState Bound { get; }
		public PlayerSlamState Slam { get; }
		public PlayerStunState Stun { get; }
		
		public MovementRegion(PlayerGroundedState groundedState, 
			PlayerAimingState aimingState,
			PlayerAirborneState airborneState,
			PlayerBoundState boundState,
			PlayerSlamState slamState,
			PlayerStunState stunState) {
			
			this.Grounded = groundedState;
			this.Aiming = aimingState;
			this.Airborne = airborneState;
			this.Bound = boundState;
			this.Slam = slamState;
			this.Stun = stunState;
		}
	}
	
	public class ActionRegion {
		public PlayerIdleState Idle { get; }
		public PlayerJabbingState Jabbing { get; }
		public PlayerAttenuateState Attenuate { get; }
		public PlayerBoundingState Bounding { get; }
		public PlayerConsumingState Consuming { get; }
		public PlayerChargeRollState ChargeRoll { get; }
		public PlayerRollingState Rolling { get; }
		public PlayerSlammingState Slamming { get; }
		public PlayerImpalingState Impaling { get; }
		public PlayerVolleyingState Volleying { get; }
		public PlayerGrabbingState Grabbing { get; }
		public PlayerYankingState Yanking { get; }
		public PlayerStunnedState Stunned { get; }
		public PlayerAbsorptionState Absorption { get; }

		
		public ActionRegion(PlayerIdleState idleState,
			PlayerJabbingState jabbingState,
			PlayerAttenuateState attenuateState,
			PlayerBoundingState boundingState,
			PlayerConsumingState consumingState,
			PlayerChargeRollState chargeRollState,
			PlayerRollingState rollingState,
			PlayerSlammingState slammingState,
			PlayerImpalingState impalingState,
			PlayerVolleyingState volleyingState,
			PlayerGrabbingState grabbingState,
			PlayerYankingState yankingState,
			PlayerStunnedState stunnedState,
			PlayerAbsorptionState absorptionState) {
			
			this.Idle = idleState;
			this.Jabbing = jabbingState;
			this.Attenuate = attenuateState;
			this.Bounding = boundingState;
			this.Consuming = consumingState;
			this.ChargeRoll = chargeRollState;
			this.Rolling = rollingState;
			this.Slamming = slammingState;
			this.Impaling = impalingState;
			this.Volleying = volleyingState;
			this.Grabbing = grabbingState;
			this.Yanking = yankingState;
			this.Stunned = stunnedState;
			this.Absorption = absorptionState;
		}
	}
	
	public class MetaRegion {
		public PlayerNormalState Normal { get; }
		public PlayerCurledState Curled { get; }
		public PlayerLockdownState Lockdown { get; }
		public PlayerAimState Aim { get; }
		
		public MetaRegion(PlayerNormalState normalState,
			PlayerCurledState curledState,
			PlayerLockdownState lockdownState,
			PlayerAimState aimState) {
			
			this.Normal = normalState;
			this.Curled = curledState;
			this.Lockdown = lockdownState;
			this.Aim = aimState;
		}
	}
	
	public class DefenseRegion {
		public PlayerDefaultState Default { get; }
		public PlayerShellState Shell { get; }
		public PlayerAbsorbingState Absorb { get; }
		
		public DefenseRegion(PlayerDefaultState defaultState,
			PlayerShellState shellState,
			PlayerAbsorbingState absorbingState) {
			
			this.Default = defaultState;
			this.Shell = shellState;
			this.Absorb = absorbingState;
		}
		
	}
	
	public MovementRegion movementStates { get; private set; }
	public ActionRegion actionStates { get; private set; }
	public MetaRegion metaStates { get; private set; }
	public DefenseRegion defenseStates { get; private set; }
	private PlayerMovementState currentMovement;
	private PlayerActionState currentAction;
	private PlayerMetaState currentMeta;
	private PlayerDefenseState currentDefense;

    // Update is called once per frame
    void Update()
    {
		if (HP <= 0) {
			hpFillImg.rectTransform.localScale = new Vector3(0, 1, 1);
			return;
		} else {
			hpFillImg.rectTransform.localScale = new Vector3(Mathf.Clamp01(HP / maxHP), 1, 1);
		}
		this.currentMeta?.Tick();
		this.currentAction?.Tick();
        this.currentMovement?.Tick();
		this.currentDefense?.Tick();
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
	
	public void MetaTransition(PlayerMetaState newState) {
		this.currentMeta?.Exit();
		this.currentMeta = newState;
		this.currentMeta?.Enter();
		Debug.Log("Transiting to " + this.currentMeta);
	}
	
	public void DefenseTransition(PlayerDefenseState newState) {
		this.currentDefense?.Exit();
		this.currentDefense = newState;
		this.currentDefense?.Enter();
		Debug.Log("Transiting to " + this.currentDefense);
	}
	
	// Input Control
	public Vector3 inputDirection;
	public bool shelled = false;
	public bool movementAllowed = true;
	[SerializeField] GameObject offenseGearWheel;
	[SerializeField] GameObject defenseGearWheel;
	[SerializeField] GameObject emulsion;
	public CinemachineCamera aimCam;
	public CinemachineOrbitalFollow freeCam;
	public CinemachineCamera emulCam;
	private int numEmulsions = 0;
	private List<GameObject> emulsionList = new List<GameObject>();
	public GameObject aimAnchor;
	public RectTransform reticle;
	private int inGearWheel = 0; // 0 = Not in Gear Wheel, 1 = Offense, 2 = Defense
	public int equippedOffense { get; private set; } = 0;
	public int equippedDefense { get; private set; } = 0;
	
	public void SetCam(int mode) {
		if (mode == 0) {
			freeCam.HorizontalAxis.Value = transform.eulerAngles.y;
			aimCam.Priority = 0;
		} else {
			aimCam.Priority = 3;
		}
	}
	public void EnterEmulsion() {
		numEmulsions++;
		if (numEmulsions != 0) {
			emulCam.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value = freeCam.HorizontalAxis.Value;
			emulCam.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value = freeCam.VerticalAxis.Value;
			emulCam.Priority = 2;
			moveSpeed = 20f;
		}
	}
	public void ExitEmulsion() {
		numEmulsions--;
		if (numEmulsions == 0) {
			freeCam.HorizontalAxis.Value = emulCam.GetComponent<CinemachineOrbitalFollow>().HorizontalAxis.Value;
			freeCam.VerticalAxis.Value = emulCam.GetComponent<CinemachineOrbitalFollow>().VerticalAxis.Value;
			emulCam.Priority = 0;
			moveSpeed = 8f;
		}
	}
	
	
	public void OnMove(InputAction.CallbackContext context) {
		Vector2 direction = context.ReadValue<Vector2>() * moveSpeed;
		this.inputDirection.x = direction.x;
		this.inputDirection.z = direction.y;
	}
	
	public void OnJump(InputAction.CallbackContext context) {
		if (!context.started) return;
		Command command = new JumpCommand();
		command = this.currentMeta.FilterCommand(command);
		command = this.currentDefense.FilterCommand(command);
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	public void JumpHelper() {
		currentVelocity.y = jumpSpeed;
	}

	public void OnJab(InputAction.CallbackContext context) {
		if (context.started) {
			Command command = new JabCommand();
			command = this.currentMeta.FilterCommand(command);
			command = this.currentDefense.FilterCommand(command);
			command = this.currentMovement.FilterCommand(command);
			command = this.currentAction.FilterCommand(command);
			command.execute(this.currentMovement, this.currentAction);
		} else if (context.canceled) {
			if (currentAction == actionStates.Volleying) {
				ActionTransition(actionStates.Idle);
			}
		}
	}
	public void RollCollideHelper(GameObject other, float dmg, float stun) {
		Debug.Log("Rolled into " + other);
		if (currentAction == actionStates.Rolling) {
			if (currentDefense == defenseStates.Shell) {
				// Don't exit roll
				// Ploughed enemies do not collide with player
				if (other.GetComponent<EnemyAI>().AttemptPlough(this)) {
					defenseStates.Shell.ploughed.Add(other.GetComponent<EnemyAI>());
					// Stun 'n' Drag the enemy 
					// Blast all ploughed enemies
					defenseStates.Shell.PloughDamage();
					// When either Shell exits or Roll ends process the plough
					return;
				}
			}
			if (actionStates.Rolling.lastHit == other) return;
			// Play animation and link state exit to machine rider?
			// If I can actually make the damn animations work zzzz
			// For now this shall suffice
			actionStates.Rolling.lastHit = other;
			actionStates.Rolling.lastHitTime = Time.time;
			other.GetComponent<EnemyAI>().takeDamage(dmg, stun, this);
			ActionTransition(actionStates.Idle);
		}
	}
	public void GrabHelper() {
		actionStates.Grabbing.target?.GetComponent<EnemyAI>()?.AttemptGrab(this);
	}
	public void YankHelper() {
		actionStates.Yanking.target?.GetComponent<EnemyAI>()?.AttemptYank(this);
	}
	
	public void OnAttenuate(InputAction.CallbackContext context) {
		if (!context.started) return;
		if (Time.time - actionStates.Attenuate.lastExit <= 1f) return; // Move CD-checking to the States later
		Command command = new AttenuateCommand();
		command = this.currentMeta.FilterCommand(command);
		command = this.currentDefense.FilterCommand(command);
		command = this.currentMovement.FilterCommand(command);
		command = this.currentAction.FilterCommand(command);
		command.execute(this.currentMovement, this.currentAction);
	}
	
	public void OnConsume(InputAction.CallbackContext context) {
		if (context.started) {
			Command command = new ConsumeCommand();
			command = this.currentMeta.FilterCommand(command);
			command = this.currentDefense.FilterCommand(command);
			command = this.currentMovement.FilterCommand(command);
			command = this.currentAction.FilterCommand(command);
			command.execute(this.currentMovement, this.currentAction);
		} else if (context.canceled) {
			if (currentAction == actionStates.ChargeRoll) {
				ActionTransition(actionStates.Rolling);
			}
			if (currentAction == actionStates.Impaling) {
				ActionTransition(actionStates.Idle);
			}
		}
	}
	
	public void OnHoldAction(InputAction.CallbackContext context) {
		PlayerMetaState nextState = metaStates.Normal;
		switch (equippedOffense) {
			case 0:
				break;
			case 1:
				nextState = metaStates.Curled;
				break;
			case 2:
				nextState = metaStates.Lockdown;
				break;
			case 3:
				nextState = metaStates.Aim;
				break;
			default:
				break;
		}
		if (context.started) {
			MetaTransition(nextState);
		} else if (context.canceled) {
			MetaTransition(metaStates.Normal);
		}
	}
	
	public void OnDefenseHold(InputAction.CallbackContext context) {
		PlayerDefenseState nextState = defenseStates.Default;
		switch (equippedDefense) {
			case 0:
				break;
			case 1:
				nextState = defenseStates.Shell;
				break;
			case 2:
				nextState = defenseStates.Absorb;
				break;
			case 3:
				if (!context.started) return;
				emulsionList.RemoveAll(item => item == null);
				if (emulsionList.Count >= 3) {
					if (emulsionList[0] != null) {
						Destroy(emulsionList[0]);
					}
					emulsionList.RemoveAt(0);
				}
				GameObject em = Transform.Instantiate(emulsion, transform.position, Quaternion.identity);
				emulsionList.Add(em);
				break;
			default:
				break;
		}
		if (context.started) {
			DefenseTransition(nextState);
		} else if (context.canceled) {
			DefenseTransition(defenseStates.Default);
		}
	}
	
	public void OnOffenseGearWheel(InputAction.CallbackContext context) {
		if (context.started) {
			if (inGearWheel != 0) return;
			inGearWheel = 1;
			Time.timeScale = 0.3f;
			// Pop up HUD
			offenseGearWheel.SetActive(true);
			// Disable camera look
			CinemachineCamera cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
			cam.GetComponent<CinemachineInputAxisController>().enabled = false;
		} else if (context.canceled) {
			if (inGearWheel != 1) return;
			inGearWheel = 0;
			Time.timeScale = 1f;
			// Close HUD and queue weapon swap
			offenseGearWheel.SetActive(false);
			// Re-enable camera look
			CinemachineCamera cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
			cam.GetComponent<CinemachineInputAxisController>().enabled = true;
			// Update Offense
			Debug.Log("Mode: " + offenseGearWheel.GetComponent<RadialMenu>().att);
			equippedOffense = offenseGearWheel.GetComponent<RadialMenu>().att;
		}
	}
	public void OnDefenseGearWheel(InputAction.CallbackContext context) {
		if (context.started) {
			if (inGearWheel != 0) return;
			inGearWheel = 2;
			Time.timeScale = 0.3f;
			// Pop up HUD
			defenseGearWheel.SetActive(true);
			// Disable camera look
			CinemachineCamera cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
			cam.GetComponent<CinemachineInputAxisController>().enabled = false;
		} else if (context.canceled) {
			if (inGearWheel != 2) return;
			inGearWheel = 0;
			Time.timeScale = 1f;
			// Close HUD and queue weapon swap
			defenseGearWheel.SetActive(false);
			// Re-enable camera look
			CinemachineCamera cam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera as CinemachineCamera;
			cam.GetComponent<CinemachineInputAxisController>().enabled = true;
			// Update Defense
			Debug.Log("Mode: " + defenseGearWheel.GetComponent<RadialMenu>().att);
			equippedDefense = defenseGearWheel.GetComponent<RadialMenu>().att;
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
	public HashSet<EnemyAI> crowd;
	
	
	public void getHit(GameObject src, float dmg) {
		if (currentAction == actionStates.Attenuate) return;
		if (currentDefense == defenseStates.Shell) dmg *= 0.7f;
		HP -= dmg;
		if (HP <= 0) {
			animator.Play("Deflate");
			GetComponent<PlayerInput>().DeactivateInput();
		}
	}
	public void BasicHeal() {
		HP += healAmt;
		HP = Mathf.Min(HP, maxHP);
	}
	
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.movementStates = new MovementRegion(
			new PlayerGroundedState(this),
			new PlayerAimingState(this),
			new PlayerAirborneState(this),
			new PlayerBoundState(this),
			new PlayerSlamState(this),
			new PlayerStunState(this)
		);
		this.actionStates = new ActionRegion(
			new PlayerIdleState(this),
			new PlayerJabbingState(this),
			new PlayerAttenuateState(this),
			new PlayerBoundingState(this),
			new PlayerConsumingState(this),
			new PlayerChargeRollState(this),
			new PlayerRollingState(this),
			new PlayerSlammingState(this),
			new PlayerImpalingState(this),
			new PlayerVolleyingState(this),
			new PlayerGrabbingState(this),
			new PlayerYankingState(this),
			new PlayerStunnedState(this),
			new PlayerAbsorptionState(this)
		);
		this.metaStates = new MetaRegion(
			new PlayerNormalState(this),
			new PlayerCurledState(this),
			new PlayerLockdownState(this),
			new PlayerAimState(this)
		);
		this.defenseStates = new DefenseRegion(
			new PlayerDefaultState(this),
			new PlayerShellState(this),
			new PlayerAbsorbingState(this)
		);
		this.inputDirection = Vector3.zero;
		this.currentVelocity = Vector3.zero;
		this.cam = Camera.main;
		this.crowd = new HashSet<EnemyAI>();
		this.controller = GetComponent<CharacterController>();
		this.animator = GetComponent<Animator>();
		this.MovementTransition(movementStates.Grounded);
		this.ActionTransition(actionStates.Idle);
		this.MetaTransition(metaStates.Normal);
		this.DefenseTransition(defenseStates.Default);
		HP += maxHP;
		
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
    }
	
	
	// Levers for external forces
	
	public void Reaction(Vector3 knockbackDir, float stunDuration) {
		MovementTransition(movementStates.Grounded);
		ActionTransition(actionStates.Idle);
		actionStates.Stunned.stunTimer = stunDuration;
		movementStates.Stun.stunTimer = stunDuration;
		movementStates.Stun.knockbackDir = knockbackDir;
		MovementTransition(movementStates.Stun);
		ActionTransition(actionStates.Stunned);
	}
	
}
