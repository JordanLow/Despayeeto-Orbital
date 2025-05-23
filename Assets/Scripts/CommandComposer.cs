using UnityEngine;

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