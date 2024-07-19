using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSkill : StateMachineBehaviour
{
    PlayerController owner;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        owner = animator.GetComponent<PlayerController>();
        owner.IsAttacking = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime >= 0.9f) owner.IsAttacking = false;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("BasicSkill");
    }
}
