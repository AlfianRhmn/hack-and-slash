using UnityEngine;

public class ZombieIdleState : StateMachineBehaviour
{
    float timer;
    public float idleTime = 0f;

    public float detectionAreaRadius = 10f;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    }
}
