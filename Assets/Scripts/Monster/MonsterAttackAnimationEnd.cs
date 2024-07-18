using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackAnimationEnd : StateMachineBehaviour
{
    readonly int HashAttack = Animator.StringToHash("Attack");

    private MonsterView owner;
    private Transform attackTarget;

    private bool isAttack;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        owner = animator.GetComponentInParent<MonsterView>();
        isAttack = false;
    }
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (owner._findTarget == null) return;
        
        attackTarget = owner._findTarget;
        float distance = Vector3.Distance(attackTarget.position, owner.transform.position);
        if (!isAttack && distance <= owner.AttackRange && stateInfo.normalizedTime >= 0.1f)
        {
            PlayerController target = attackTarget.GetComponent<PlayerController>();
            if (target == null) return;
          
            if(target.currentPlayerTag == "Human")
            {
                DBCharacterPC.Instance.HitDamageHuman(owner.AttackDamage);
            }
            else
            {
                DBCharacterPC.Instance.HitDamageWolf(owner.AttackDamage);
            }

            isAttack = true;
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(HashAttack);
    }
}
