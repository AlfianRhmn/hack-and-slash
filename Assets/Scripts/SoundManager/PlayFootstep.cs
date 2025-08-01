using SmallHedge.SoundManager;
using UnityEngine;

public class PlayFootstep : MonoBehaviour
{
    public Weapon sword;
    public Weapon rightLeg;
    public void PlaySound()
    {
        SoundManager.PlaySound(SoundType.Footstep);
    }

    public void CheckHit()
    {
        sword.DoHit(); 
    }

    public void CheckRightLeg()
    {
        rightLeg.DoHit();
    }
}
