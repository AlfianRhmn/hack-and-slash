using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
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
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            print(enemy.gameObject.name + " took " + damage + " damage!");
            //enemy.currentHealth -= damage;
            /*
             * if (enemy.currentHealth <= 0)
             * {
             *      Destroy(enemy.gameObject);
             * }
            */
        }
    }

    public void EnableHitbox()
    {
        hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        hitbox.enabled = false;
    }
}
