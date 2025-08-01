using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static SpecialEffects;

public class Weapon : MonoBehaviour
{
    public LayerMask hitLayers;
    public float damage = 10;
    public float hitboxRange = 1.0f;
    public Vector3 hitboxSize = new Vector3(0.5f, 1f, 0.5f);
    ObjectPooling damageNumber;
    [Range(0f, 100f)]
    public float critChance;
    public float critDamage;

    private void Start()
    {
        damageNumber = PlayerManager.Instance.damageNumber;
    }

    public void DoHit()
    {
        Vector3 center = transform.position + transform.forward * hitboxRange;

        Collider[] hits = Physics.OverlapBox(center, hitboxSize / 2, transform.rotation, hitLayers);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyBehaviour enemy = hit.GetComponent<EnemyBehaviour>();
                if (enemy != null)
                {
                    PlayerManager.Instance.combat.ResetTimer();
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
                    enemy.TakeDamage(totalDamage);
                    if (PlayerManager.Instance.onAir)
                    {
                        enemy.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
                    }
                }
            }
        }
    }

    // Optional debug gizmo
    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + transform.forward * hitboxRange;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, hitboxSize);
    }
}
