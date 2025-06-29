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
		FSM.animator.Play(FSM.attackProfile.jabCombo.comboStages[comboStage].animationClip.name, 2);
		comboStage = (comboStage + 1) % 3;
	}
}

public class LargeChainDelegate : IAttackDelegate
{
	private int comboStage = 0; // 0 = Left Jab, 1 = Right Jab, 2 = Chain Finisher
	private float chainWindow = -1f;

	public void Execute(PlayerStateMachine FSM) {
		if (chainWindow != -1f && Time.time - chainWindow >= 2f) {
			comboStage = 0;
		}
		chainWindow = Time.time;
		FSM.animator.Play(FSM.attackProfile.largeJabCombo.comboStages[comboStage].animationClip.name, 2);
		comboStage = (comboStage + 1) % 3;
	}
}

public class PierceChainDelegate : IAttackDelegate
{
	private int comboStage = 0; // 0 = Left Jab, 1 = Right Jab
	private float chainWindow = -1f;

	public void Execute(PlayerStateMachine FSM) {
		if (chainWindow != -1f && Time.time - chainWindow >= 2f) {
			comboStage = 0;
		}
		chainWindow = Time.time;
		FSM.animator.Play(FSM.attackProfile.pierceCombo.comboStages[comboStage].animationClip.name, 2);
		comboStage = (comboStage + 1) % 2;
	}
}

public class SlashChainDelegate : IAttackDelegate
{
	private int comboStage = 0; // 0 = R-L Slash, 1 = L-R Slash, 2 = Spin Slash
	private float chainWindow = -1f;

	public void Execute(PlayerStateMachine FSM) {
		if (chainWindow != -1f && Time.time - chainWindow >= 2f) {
			comboStage = 0;
		}
		chainWindow = Time.time;
		Debug.Log(FSM.attackProfile.slashCombo.comboStages[comboStage].animationClip.name);
		FSM.animator.Play(FSM.attackProfile.slashCombo.comboStages[comboStage].animationClip.name, 2);
		comboStage = (comboStage + 1) % 3;
	}
}