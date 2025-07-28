using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Special Attack")]
public class SkillSO : ScriptableObject
{
    [Header("General")]
    public string attackName;
    public AnimatorOverrideController animOV;
    public float damage;
    [Header("Skills")]
    public Sprite skillIcon;
    public float manaCost;
    public enum typeOfSkill { Fireball, GiveStatus, Heal, Quake}
    public typeOfSkill[] skillType; // fireball, giveStatus, heal
    public float timeBeforeApply;
    [Header("Skill - Fireball")]
    public GameObject projectile;
    public float velocity;
    [Header("Skill - Give Status")]
    public StatusEffects[] status;
    [Header("Skill - Heal")]
    public float heal;
}
