using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    public LayerMask hitLayers;
    public float damage = 10;
    public float hitboxRange = 1.0f;
    public Vector3 hitboxSize = new Vector3(0.5f, 1f, 0.5f);
    public Transform mainBody;

    public void DoHit()
    {
        Vector3 center = transform.position + transform.forward * hitboxRange;

        Collider[] hits = Physics.OverlapBox(center, hitboxSize / 2, transform.rotation, hitLayers);

        if (Vector3.Distance(PlayerManager.Instance.transform.position, transform.position) < 3)
        {
            if (PlayerManager.Instance.invulnerability)
            {
                PlayerManager.Instance.combat.TakeDamage(0, mainBody);
            }
        }

        HashSet<PlayerCombat> alreadyHit = new HashSet<PlayerCombat>();

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerCombat player = hit.GetComponent<PlayerCombat>();
                if (player != null && !alreadyHit.Contains(player))
                {
                    alreadyHit.Add(player);
                    player.TakeDamage(damage, mainBody);
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
