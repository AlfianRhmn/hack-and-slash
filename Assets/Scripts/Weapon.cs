using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int effectID; // 0 - normal weapon, 1 - quake
    public float damage;
    public bool repeatingDamage;
    [UnityEngine.Range(0, 100f)]
    public float critChance = 50f;
    public float critDamage = 50f;
    public List<GameObject> targets;
    public ObjectPooling damageNumber;
    BoxCollider hitbox;
    [Header("General - Skills")]
    public float duration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.enabled = false;
        if (damageNumber == null)
        {
            damageNumber = GameObject.FindWithTag("Damage Number").GetComponent<ObjectPooling>();
        }
    }

    private void Update()
    {
        if (effectID != 0)
        {
            duration -= Time.deltaTime;
            if (duration < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.name);
        EnemyBehaviour enemy = other.GetComponent<EnemyBehaviour>();
        if (enemy != null)
        {
            if (!targets.Contains(enemy.gameObject) || repeatingDamage)
            {
                PlayerManager.Instance.combat.ultimateProgress += Random.Range(1, 5);
                float totalDamage = damage;
                AlwaysLookAt look = damageNumber.GetObject().GetComponent<AlwaysLookAt>();
                look.sourceOfPool = damageNumber;
                look.transform.position = enemy.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 3f), Random.Range(-1f, 1f));
                look.transform.localScale = new Vector3(0.2445875f, 0.2445875f, 0.2445875f);
                look.transform.GetChild(0).GetComponent<TextMeshPro>().text = Mathf.RoundToInt(damage).ToString();
                look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.white;
                if (Random.Range(1, 101) <= critChance)
                {
                    //Critical Hits!
                    totalDamage = damage + (damage * (critDamage / 100f));
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().text = Mathf.RoundToInt(totalDamage).ToString();
                    look.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.yellow;
                }
                targets.Add(enemy.gameObject);
                enemy.TakeDamage(totalDamage);
                if (PlayerManager.Instance.onAir)
                {
                    enemy.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
                }
                switch (effectID)
                {
                    case 1:
                        StartCoroutine(enemy.LaunchEnemy(0.6f, 2.5f));
                        break;
                }
            }
        }
    }

    public void EnableHitbox()
    {
        if (GetComponent<TrailRenderer>() != null)
        {
            GetComponent<TrailRenderer>().enabled = true;
        }
        hitbox.enabled = true;
        targets.Clear();
    }

    public void DisableHitbox()
    {
        if (GetComponent<TrailRenderer>() != null)
        {
            GetComponent<TrailRenderer>().enabled = false;
        }
        hitbox.enabled = false;
    }
}
