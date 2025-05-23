using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttack", menuName = "Scriptable Objects/PlayerAttack")]
public class PlayerAttack : ScriptableObject
{
    public AnimationClip animationClip;
    public float damage;
	public string attackName;
}
