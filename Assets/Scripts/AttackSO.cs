using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Basic Attack")]
public class AttackSO : ScriptableObject
{
    [Header("General")]
    public string attackName;
    public AnimatorOverrideController animOV;
    public float damage;
    public Movement[] movementDone;
    [Tooltip("Wait how long until this animation runs (s). The higher, the longer it takes to start this animation.")]
    public float timeToNextAnim; // 0 to 1;
}

[System.Serializable]
public class StatusEffects
{
    public enum statusType { attack, defense, critDMG, critRate, poison}
    public statusType type;
    public string statusName;
    public string statusDescription;
    public Sprite statusIcon;
    public float severity; // for attack/defense, 1 represents 100% increase and 0.2 represents 20% increase.  --- for critdmg/critrate, 20 means 20% added to the rate --- for poison, severity is the damage given per second
    public float duration;
    public float maxDuration;

    public StatusEffects Copy()
    {
        StatusEffects copy = new StatusEffects();
        copy.type = type;
        copy.statusName = statusName;
        copy.statusDescription = statusDescription;
        copy.duration = duration;
        copy.maxDuration = maxDuration;
        copy.statusIcon = statusIcon;
        copy.severity = severity;
        return copy;
    }
}