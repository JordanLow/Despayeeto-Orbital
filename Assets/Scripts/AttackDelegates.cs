using UnityEngine;
using System;
// Scuffed async placeholder
using System.Threading.Tasks;

public interface IAttackDelegate 
{
	public bool isFinished { get; }

	public void Execute(PlayerStateMachine ctx);
}

public class RapidsDelegate : IAttackDelegate
{
	public bool isFinished { get; private set; } = false;
	private PlayerStateMachine ctx;
	private int metaState = 0; // 0 = Left Jab, 1 = Right Jab

	public async void Execute(PlayerStateMachine curCtx) {
		ctx = curCtx;
		isFinished = false;
		if (metaState == 0) {
			metaState = 1;
			ctx.leftPunchPlaceholder.SetActive(true);
		} else if (metaState == 1) {
			metaState = 0;
			ctx.rightPunchPlaceholder.SetActive(true);
		}
		// Use animation events to control time properly
		// For now async hack gogogo
		await Task.Delay(TimeSpan.FromSeconds(1f));
		FinishAttack();
	}

	// Placeholders for simulated animated time

	private void FinishAttack() {
		if (metaState == 0) {
			ctx.rightPunchPlaceholder.SetActive(false);
		} else if (metaState == 1) {
			ctx.leftPunchPlaceholder.SetActive(false);
		}
		isFinished = true;
		ctx = null;
	}
}

public class GentlemanDelegate : IAttackDelegate
{
	public bool isFinished { get; private set; } = false;
	private PlayerStateMachine ctx;

	public async void Execute(PlayerStateMachine curCtx) {
		ctx = curCtx;
		isFinished = false;
		ctx.gentlemanPlaceholder.SetActive(true);
		// Use animation events to control time properly
		// For now async hack gogogo
		await Task.Delay(TimeSpan.FromSeconds(1f));
		FinishAttack();
	}


	private void FinishAttack() {
		ctx.gentlemanPlaceholder.SetActive(false);
		isFinished = true;
		ctx = null;
	}
}