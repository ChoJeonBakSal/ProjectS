using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackAnimationEnd : StateMachineBehaviour
{
    readonly int HashAttack = Animator.StringToHash("Attack");
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(HashAttack);
    }
}
