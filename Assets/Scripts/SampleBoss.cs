using UnityEngine;
using UnityEngine.AI;


// Refactor into extensible for different enemy types in the future
public class SampleBoss : EnemyAI
{
	[SerializeField] private GameObject unlock;
	
	public override void UnlockPower() {
		unlock.SetActive(true);
	}
}
