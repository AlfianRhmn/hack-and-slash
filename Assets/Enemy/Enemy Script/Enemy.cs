using UnityEngine;

public class Enemy : MonoBehaviour
{
    public WeaponEnemy axe;

    public int enemyDamage;

    private void Start()
    {
        axe.damage = enemyDamage;
    }
}
