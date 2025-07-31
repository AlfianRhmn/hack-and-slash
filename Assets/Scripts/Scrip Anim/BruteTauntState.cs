using UnityEngine;

public class BruteTauntState : StateMachineBehaviour
{
    Transform player;
    public float tauntDuration = 2f;
    private float tauntEndTime;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        tauntEndTime = Time.time + tauntDuration;

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        // Check if the taunt duration has ended
        if (Time.time >= tauntEndTime)
        {
            // Transition back to the run state or idle state
            animator.SetBool("RUN", true);
            animator.SetBool("ATTACK", true);
            if (!animator.GetBool("ATTACK ENRAGED"))
            {
                animator.Play("Standing Taunt Battlecry");
                animator.SetBool("ATTACK ENRAGED", true);
            }
        }
        else
        {
            // Continue taunting (you can add additional logic here if needed)
            animator.transform.LookAt(player);
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       
    }

}
