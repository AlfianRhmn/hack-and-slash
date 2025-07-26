using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public List<GameObject> targets;
    BoxCollider hitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.enabled = false;
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        Balmond enemy = other.GetComponent<Balmond>();
        if (enemy != null)
        {
            if (!targets.Contains(enemy.gameObject))
            {
                targets.Add(enemy.gameObject);
                enemy.TakeDamage(damage);
            }
        }
    }

    public void EnableHitbox()
    {
        hitbox.enabled = true;
        targets.Clear();
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }
}
