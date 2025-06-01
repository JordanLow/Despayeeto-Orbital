using UnityEngine;
using System;
// Scuffed async placeholder
using System.Threading.Tasks;

public interface IAttackDelegate 
{
	public void Execute(PlayerStateMachine FSM);
}

public class BasicChainDelegate : IAttackDelegate
{
	private int comboStage = 0; // 0 = Left Jab, 1 = Right Jab, 2 = Chain Finisher
	private float chainWindow = -1f;

	public void Execute(PlayerStateMachine FSM) {
		if (chainWindow != -1f && Time.time - chainWindow >= 2f) {
			comboStage = 0;
		}
		chainWindow = Time.time;
		FSM.animator.Play(FSM.attackProfile.jabCombo.comboStages[comboStage].animationClip.name, 1);
		comboStage = (comboStage + 1) % 3;
	}
}

public class GentlemanDelegate : IAttackDelegate
{
	public bool isFinished { get; private set; } = false;
	private PlayerStateMachine ctx;

	public async void Execute(PlayerStateMachine curCtx) {
		Debug.Log("Deprecated Attack");
		await Task.Delay(TimeSpan.FromSeconds(1f));
		FinishAttack();
	}


	private void FinishAttack() {
		Debug.Log("Get off this mode");
	}
}