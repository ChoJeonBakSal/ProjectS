using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : StateMachineBehaviour
{
    PlayerController owner;
    private BoxCollider collider;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        owner = animator.GetComponent<PlayerController>();
        collider = owner.GetComponentInChildren<BoxCollider>();
        owner.IsAttacking = true;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(stateInfo.normalizedTime >= 0.09f && stateInfo.normalizedTime <= 0.3f)
        {
            collider.enabled = true;
        }
        else
        {
            collider.enabled = false;
        }

        if(stateInfo.normalizedTime >= 1f) owner.IsAttacking = false;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("BasicAtk");
    }
}
