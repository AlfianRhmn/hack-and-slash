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
    public Movement[] movePlayer;
    public float waitingUltimateInitiation;
}

[System.Serializable]
public class Movement
{
    public float timeBeforeMoving;
    public float amountToMove;
    public Vector3 moveDirection;
}
