using UnityEngine;

// I wish C# was a functional language...

public interface Command
{
	public abstract void execute(PlayerMovementState movementState, PlayerActionState actionState);
}

public class VoidCommand : Command 
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) { }
}

public class JumpCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnJumpCommand();
		actionState.OnJumpCommand();
	}
}

public class JabCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnJabCommand();
		actionState.OnJabCommand();
	}
}

public class AttenuateCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnAttenuateCommand();
		actionState.OnAttenuateCommand();
	}
}

public class BoundCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnBoundCommand();
		actionState.OnBoundCommand();
	}
}

public class ConsumeCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnConsumeCommand();
		actionState.OnConsumeCommand();
	}
}

public class ChargeRollCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnChargeRollCommand();
		actionState.OnChargeRollCommand();
	}
}

public class SlamCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnSlamCommand();
		actionState.OnSlamCommand();
	}
}

public class SpineCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnSpineCommand();
		actionState.OnSpineCommand();
	}
}

public class VolleyCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnVolleyCommand();
		actionState.OnVolleyCommand();
	}
}

public class ImpaleCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnImpaleCommand();
		actionState.OnImpaleCommand();
	}
}

public class GrabCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnGrabCommand();
		actionState.OnGrabCommand();
	}
}

public class YankCommand : Command
{
	public void execute(PlayerMovementState movementState, PlayerActionState actionState) {
		movementState.OnYankCommand();
		actionState.OnYankCommand();
	}
}