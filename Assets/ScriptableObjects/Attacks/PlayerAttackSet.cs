using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackSet", menuName = "Scriptable Objects/PlayerAttackSet")]
public class PlayerAttackSet : ScriptableObject
{
    public PlayerCombo jabCombo;
	public PlayerCombo largeJabCombo;
	public PlayerCombo pierceCombo;
	public PlayerCombo slashCombo;
	public GameObject spineProjectile;
	public GameObject impaleSeeker;
}
