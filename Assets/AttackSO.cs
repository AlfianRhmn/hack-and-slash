using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Basic Attack")]
public class AttackSO : ScriptableObject
{
    [Header("General")]
    public string attackName;
    public AnimatorOverrideController animOV;
    public float damage;
    [Tooltip("Preferably around 0.1 to 1, depends on animation")]
    public float timeToNextAnim; // 0 to 1;
    [Header("Buff")]
    public StatusEffects[] status;
    [Header("Heal")]
    public float heal;
}

[System.Serializable]
public class StatusEffects
{
    public enum statusType { attack, defense, critdmg, critrate, poison}
    public statusType type;
    public float severity; // for attack/defense, 1 represents 100% increase and 0.2 represents 20% increase.  --- for critdmg/critrate, 20 means 20% added to the rate --- for poison, severity is the damage given per second
}
