using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Ultimate")]
public class UltimateSO : ScriptableObject
{
    [Header("General")]
    public string attackName;
    public AnimatorOverrideController animOV;
    public float damage;
    [Header("Skills")]
    public Sprite skillIcon;
}
