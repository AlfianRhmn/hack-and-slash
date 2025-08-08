using UnityEngine;

public class PlayPhaseVFX : StateMachineBehaviour
{
    [Tooltip("Assign your VFX prefab here")]
    public GameObject vfxPrefab;

    private GameObject spawnedVFX;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 1) grab a scene-instance transform (here, the object the Animator sits on)
        Transform parentTransform = animator.transform;

        // 2) instantiate at that position, parented under it
        spawnedVFX = Object.Instantiate(
            vfxPrefab,
            parentTransform.position,
            Quaternion.identity,
            parentTransform
        );
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (spawnedVFX != null)
            Object.Destroy(spawnedVFX);
    }
}

