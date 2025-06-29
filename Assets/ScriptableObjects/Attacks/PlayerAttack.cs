using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttack", menuName = "Scriptable Objects/PlayerAttack")]
public class PlayerAttack : ScriptableObject
{
    public AnimationClip animationClip;
    public float damage;
	public float stun;
	public string attackName;
}
