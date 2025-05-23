using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerCombo", menuName = "Scriptable Objects/PlayerCombo")]
public class PlayerCombo : ScriptableObject
{
    public List<PlayerAttack> comboStages;
}
