using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum AttackType
{
    normalAttack,
    Skill_A,
    Skill_B
}

public class BossView : MonoBehaviour
{
    [SerializeField] private float MonsterHP;
    [SerializeField] private float AttackRange;
    [SerializeField] private float AttackDelay;

    MonsterBTRunner _monsterBTRunner;

    private Transform _players;
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;
    private Collider monsterCollider;

    private float _AttackDelayTimer;
    private bool isAttack;
    private bool isHurt;
    private bool isDie;
    private bool isDead;

    private void Awake()
    {
        _monsterBTRunner = new MonsterBTRunner(SetMonsterBT());
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        monsterCollider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        _AttackDelayTimer = 0;
        isAttack = false;
    }

    IBTNode SetMonsterBT()
    {
        var DieNodeList = new List<IBTNode>();
        DieNodeList.Add(new MonsterActionNode(CheckHPZeroOnUpdate));
        DieNodeList.Add(new MonsterActionNode(CheckRunningDieAnimationOnUpdate));
        var DieSeqNode = new MonsterSequenceNode(DieNodeList);

        var HurtNodeList = new List<IBTNode>();
        HurtNodeList.Add(new MonsterActionNode(StartHurtAnimationOnUpdate));
        HurtNodeList.Add(new MonsterActionNode(CheckRunningHurtAnimationOnUpdate));
        var HurtSeqNode = new MonsterSequenceNode(HurtNodeList);

        var FollowNodeList = new List<IBTNode>();
        FollowNodeList.Add(new MonsterActionNode(CheckFollowingRangeOnUpdate));
        var FollowSeqNode = new MonsterSequenceNode(FollowNodeList);

        var AttackNodeList = new List<IBTNode>();
        var AttackSeqNode = new MonsterSequenceNode(AttackNodeList);

        List<IBTNode> rootSelectNode = new List<IBTNode>();
        rootSelectNode.Add(DieSeqNode);
        rootSelectNode.Add(HurtSeqNode);
        rootSelectNode.Add(FollowSeqNode);
        rootSelectNode.Add(AttackSeqNode);

        var rootNode = new MonsterSelectorNode(rootSelectNode);
        return rootNode;
    }

    #region Follow
    private void MoveToTarget(Vector3 targetTransform)
    {
        agent.SetDestination(targetTransform);
    }

    IBTNode.EBTNodeState CheckFollowingRangeOnUpdate()
    {
        if (isAttack) return IBTNode.EBTNodeState.Fail;

        //타겟과의 거리 파악
        float distance = Vector3.Distance(transform.position, _players.position);

        //공격 딜레이 타이머 증가(감소)
        _AttackDelayTimer = Mathf.Clamp(_AttackDelayTimer + Time.deltaTime, 0f, AttackDelay);

        if (distance > AttackRange)
        {
            agent.stoppingDistance = AttackRange;
            //이동 애니메이션 실행
            MoveToTarget(_players.position);
        }
        else 
        {
            //이동 애니메이션 종료
        }

        if (_AttackDelayTimer >= AttackDelay)
        {
            //공격 상태로 변환
            _AttackDelayTimer = 0;
            isAttack = true;
            return IBTNode.EBTNodeState.Success;
        }
        else return IBTNode.EBTNodeState.Running;
    }
    #endregion

    #region Attack

    IBTNode.EBTNodeState DecideAttackMethodOnUpdate()
    {
        if (!isAttack) return IBTNode.EBTNodeState.Fail;

        //공격 타입 인덱스 결정
        return IBTNode.EBTNodeState.Success;
    }

    private bool IsAnimationRunning(string animationName) 
    {
        if (animator == null) return false;

        bool isRunning = false;
        var currentAnimationStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (currentAnimationStateInfo.IsName(animationName))
        {
            float normalizedTime = currentAnimationStateInfo.normalizedTime;
            isRunning = normalizedTime >= 0 && normalizedTime < 1.0f;
        }

        return isRunning;
    }

    #endregion
    #region Hurt
    public void HurtDamage(float damage, Transform attacker)
    {
        MonsterHP -= damage;

        if (MonsterHP <= 0)
        {
            isDie = true;
        }
        else
        {
            isHurt = true;
            _players = attacker;
        }
    }

    public IBTNode.EBTNodeState StartHurtAnimationOnUpdate()
    {
        if (isDie) return IBTNode.EBTNodeState.Fail;

        if (isHurt)
        {
            animator.SetTrigger("Hurt");

            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
            {
                return IBTNode.EBTNodeState.Success;
            }
            else
            {
                return IBTNode.EBTNodeState.Running;
            }
        }

        return IBTNode.EBTNodeState.Fail;
    }

    public IBTNode.EBTNodeState CheckRunningHurtAnimationOnUpdate()
    {
        if (isDie) return IBTNode.EBTNodeState.Fail;
        if (!isHurt) return IBTNode.EBTNodeState.Fail;

        if (IsAnimationRunning("Hurt"))
        {
            Debug.Log("Hurt 실행 중...");
            agent.speed = 0;
            return IBTNode.EBTNodeState.Running;
        }

        isHurt = false;
        agent.speed = 1.5f;
        return IBTNode.EBTNodeState.Success;
    }
    #endregion
    #region Die
    public IBTNode.EBTNodeState CheckHPZeroOnUpdate()
    {
        if (isDead)
        {
            rb.isKinematic = true;
            monsterCollider.enabled = false;
            agent.speed = 0f;
            agent.angularSpeed = 0f;
            return IBTNode.EBTNodeState.Success;
        }

        if (isDie)
        {
            animator.SetTrigger("Die");
            isDead = true;
        }

        return IBTNode.EBTNodeState.Fail;
    }

    public IBTNode.EBTNodeState CheckRunningDieAnimationOnUpdate()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
        {
            if (IsAnimationRunning("Die"))
            {
                return IBTNode.EBTNodeState.Running;
            }

            //Destroy(gameObject);
            Debug.Log("End");
            //StartCoroutine(DeadMonsterDestroy());

            return IBTNode.EBTNodeState.Success;
        }
        else
            return IBTNode.EBTNodeState.Running;
    }
    #endregion   
}
