using SmallHedge.SoundManager;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Special Attack")]
public class SkillSO : ScriptableObject
{
    [Header("General")]
    public string attackName;
    public AnimatorOverrideController animOV;
    public float duration;
    public float damage;
    [Header("Skills")]
    public Sprite skillIcon;
    public float manaCost;
    public Sounds[] soundUsed;
    public enum typeOfSkill { Fireball, GiveStatus, Heal, Quake}
    public typeOfSkill[] skillType; // fireball, giveStatus, heal
    public GameObject vfx;
    public float timeBeforeApply;
    [Header("Skill - Fireball")]
    public GameObject projectile;
    public float velocity;
    [Header("Skill - Give Status")]
    public StatusEffects[] status;
    [Header("Skill - Heal")]
    public float heal;
}

[System.Serializable]
public class Sounds
{
    public SoundType type;
    public float time;
}
