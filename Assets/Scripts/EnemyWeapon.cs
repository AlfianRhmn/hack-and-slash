using System.Collections;
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

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerCombat player = hit.GetComponent<PlayerCombat>();
                if (player != null)
                {
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
