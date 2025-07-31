using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Moveset")]
public class MovesetSO : ScriptableObject
{
    public string movesetName;
    public bool isAirAttack;
    public Combo[] comboList;
}

[System.Serializable]
public class Combo
{
    public enum attackTypes { TapLightAttack, TapHeavyAttack, HoldLightAttack, HoldHeavyAttack}
    public attackTypes keyUsed;
    public AttackSO attackUsed;
}
