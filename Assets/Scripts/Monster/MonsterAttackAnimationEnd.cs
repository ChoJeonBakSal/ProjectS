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
            Animator targetAnimator = target.GetComponent<Animator>();

            if (targetAnimator.GetBool("DeadChk") == true) return;  // 이미 죽은 상태인지 확인
            
            if (target.currentPlayerTag == "Human")
            {
                float Diechk  = DBCharacterPC.Instance.HitDamageHuman(owner.AttackDamage);
                if (Diechk <= 360)
                {
    
                    //target.enabled = false;
                    GameObject targethuman = GameObject.Find("Human");
                    targethuman.GetComponent<Animator>().SetTrigger("Dead");
                    targethuman.GetComponent<Animator>().SetBool("DeadChk", true);
                    targethuman.GetComponent<PlayerController>().enabled = false;

                    GameObject targetwolf = GameObject.Find("Wolf");
                    targetwolf.GetComponent<Animator>().SetTrigger("Dead");
                    targetwolf.GetComponent<Animator>().SetBool("DeadChk", true);
                    targetwolf.GetComponent<PlayerController>().enabled = false;
                }
            }
            else
            {
                float Diechk = DBCharacterPC.Instance.HitDamageWolf(owner.AttackDamage);
                if (Diechk <= 360)
                {
                    GameObject targethuman = GameObject.Find("Human");
                    targethuman.GetComponent<Animator>().SetTrigger("Dead");
                    targethuman.GetComponent<Animator>().SetBool("DeadChk", true);
                    targethuman.GetComponent<PlayerController>().enabled = false;
                    GameObject targetwolf = GameObject.Find("Wolf");
                    targetwolf.GetComponent<Animator>().SetTrigger("Dead");
                    targetwolf.GetComponent<Animator>().SetBool("DeadChk", true);
                    targetwolf.GetComponent<PlayerController>().enabled = false;

                }
            }



            isAttack = true;
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(HashAttack);
    }
}
