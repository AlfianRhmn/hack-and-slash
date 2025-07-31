using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Attacks/Enemy Attacks")]
public class EnemyMoveset : ScriptableObject
{
    public string attackName; // ini kalo mau namain attacknya, cuman sebagai dekorasi
    public AnimatorOverrideController animOV; // ini buat ganti animasi serangan musuh, jadi setiap mau nyerang animasinya diganti
    public float duration;
    public float damage; // self-explanatory, damage
    [Range(0, 100)]
    public int probability; // berapa persen serangan ini akan dipilih --- PASTIKAN KALAU TOTAL SEMUA PROBABILITY DALAM MOVESET ITU 100% 
    public float[] timeBeforeHitCheck; // nunggu berapa detik setelah animasi jalan untuk mulai hitbox check
}
