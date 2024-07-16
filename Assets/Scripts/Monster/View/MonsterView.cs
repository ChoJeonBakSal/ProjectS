using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterView : MonoBehaviour
{
    [Header("Monster_Info")]
    [SerializeField] private float MonsterAttakRange;
    [SerializeField] private float MonsterAttackDelay;
    [SerializeField] private float MonsterPatrolDelayTimeAverage;
    [SerializeField] private float MonsterHP;
    [SerializeField] private float MonsterATK;

    [SerializeField] Transform _findTarget;
    [SerializeField] LayerMask GroundLayer;

    MonsterBTRunner _monsterBTRunner;
    MonsterDetectZone detectZone;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 PatrolPoint;

    private readonly int HashAttack = Animator.StringToHash("Attack");
    private readonly int HashHurt = Animator.StringToHash("Hurt");
    private readonly int HashDie = Animator.StringToHash("Die");
    private readonly int HashMove = Animator.StringToHash("Move");

    private float _PatrolWaitTimer = 0;
    private float _AttackDelayTimer = 0;
    private float _monsterPatrolDelay;
    private bool isAttacking;
    private bool isHurt;
    private bool isDie;
    private bool isDead;
    private bool isDestroy;

    private void Awake()
    {
        _monsterBTRunner = new MonsterBTRunner(SetMonsterBT());
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        detectZone = GetComponentInChildren<MonsterDetectZone>();
        isAttacking = false;
        isHurt = false;
        isDie = false;
        isDead = false;
        isDestroy = false;

        agent.speed = 1.5f;

        if (detectZone!= null)
        {
            detectZone.OnTargetChanged += SetTarget;
        }

        _monsterPatrolDelay = Random.Range(MonsterPatrolDelayTimeAverage - 1, MonsterPatrolDelayTimeAverage + 1);
        MonsterSpawnManager.Instance.AddMonsterList(this);
    }

    private void OnDisable()
    {
        if(detectZone != null)
        {
            detectZone.OnTargetChanged -= SetTarget;
        }        

        MonsterSpawnManager.Instance.RemoveMonsterList(this);
    }

    private void SetTarget(Transform target)
    {
        _findTarget = target;
    }

    private void Update()
    {
        _monsterBTRunner.Execute();

        if(_findTarget != null && (!isDead || !isDie))
        {
            Vector3 direction = (_findTarget.position - animator.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }
        }
    }

    IBTNode SetMonsterBT()
    {
        var IdleNodeList = new List<IBTNode>();
        IdleNodeList.Add(new MonsterActionNode(WaitPatrolDelay));      //패트롤할 딜레이 기다리기
        IdleNodeList.Add(new MonsterActionNode(RandomPatrolOnUpdate));      //패트롤할 위치 받기
        IdleNodeList.Add(new MonsterActionNode(PatrolMoveOnUpdate));    //지정된 위치까지 패트롤 이동

        var IdleSeqNode = new MonsterSequenceNode(IdleNodeList);

        var followNodeList = new List<IBTNode>();
        followNodeList.Add(new MonsterActionNode(CheckFollowingRangeOnUpdate));    //플레이어 따라가기
        followNodeList.Add(new MonsterActionNode(CompleteFollowOnUpdate));    //플레이어가 공격 사거리에 들어왔는지 판단

        //Follow  상태일때, 순차적으로 실행할 이벤트 리스트들을 담는 시퀀스 노드 
        var followSeqNode = new MonsterSequenceNode(followNodeList);

        var HurtNodeList = new List<IBTNode>();
        HurtNodeList.Add(new MonsterActionNode(StartHurtAnimationOnUpdate));  //Hurt 애니메이션 실행
        HurtNodeList.Add(new MonsterActionNode(CheckRunningHurtAnimationOnUpdate));  //Hurt 애니메이션이 실행되고 있는지 확인

        var HurtSeqNode = new MonsterSequenceNode(HurtNodeList);

        var DeadNodeList = new List<IBTNode>();
        DeadNodeList.Add(new MonsterActionNode(CheckHPZeroOnUpdate));  // HP <= 0 인지 확인
        DeadNodeList.Add(new MonsterActionNode(CheckRunningDieAnimationOnUpdate));  // Monster 사망 애니메이션이 끝나면 Distory

        var DeadSeqNode = new MonsterSequenceNode(DeadNodeList);

        List<IBTNode> rootSelectNode = new List<IBTNode>();
        rootSelectNode.Add(IdleSeqNode);
        rootSelectNode.Add(followSeqNode);
        rootSelectNode.Add(HurtSeqNode);
        rootSelectNode.Add(DeadSeqNode);

        var rootNode = new MonsterSelectorNode(rootSelectNode);
        return rootNode;
    }

    #region Follow 

    //타겟을 향해 이동
    private void MoveToTarget(Vector3 targetTransform)
    {
        if (isAttacking || isHurt || isDie) return;

        agent.SetDestination(targetTransform);        
    }

    //이동하며 타겟까지 남은 거리를 업데이트
    public IBTNode.EBTNodeState CheckFollowingRangeOnUpdate()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;

        float distance = Vector3.Distance(_findTarget.position, transform.position);

        _AttackDelayTimer = Mathf.Clamp(_AttackDelayTimer + Time.deltaTime, 0f, MonsterAttackDelay);

        if (distance > MonsterAttakRange)
        {
            agent.stoppingDistance = MonsterAttakRange;
            animator.SetBool(HashMove, true);
            MoveToTarget(_findTarget.position);
            return IBTNode.EBTNodeState.Running;
        }

        //공격 애니메이션 실행
        animator.SetBool(HashMove, false);

        if (_AttackDelayTimer >= MonsterAttackDelay)
        {
            animator.SetTrigger(HashAttack);
            isAttacking = true;
            _AttackDelayTimer = 0;
            return IBTNode.EBTNodeState.Success;
        } else return IBTNode.EBTNodeState.Running;          
    }

    //공격 동작이 끝났는지 확인
    public IBTNode.EBTNodeState CompleteFollowOnUpdate()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;

        if (IsAnimationRunning("Attack"))
        {
            return IBTNode.EBTNodeState.Running;
        }

        //animator.CrossFade(HashAttack, MonsterAttackDelay);
        isAttacking = false;
        return IBTNode.EBTNodeState.Success;
    }

    private bool IsAnimationRunning(string animationName)
    {
        if(animator == null) return false;

        bool isRunning = false;
        var aaa = animator.GetCurrentAnimatorStateInfo(0);

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            isRunning = normalizedTime >= 0 && normalizedTime < 1.0f;
        }

        return isRunning;
    }
    #endregion
    #region Idle

    //패트롤 딜레이 기다리기
    public IBTNode.EBTNodeState WaitPatrolDelay()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;
        if (_findTarget != null) return IBTNode.EBTNodeState.Fail;

        if (_PatrolWaitTimer < _monsterPatrolDelay)
        {
            _PatrolWaitTimer += Time.deltaTime;
            return IBTNode.EBTNodeState.Running;
        }
        else
        {
            _PatrolWaitTimer = 0f;
            _monsterPatrolDelay = Random.Range(MonsterPatrolDelayTimeAverage - 1, MonsterPatrolDelayTimeAverage + 1);
            return IBTNode.EBTNodeState.Success;
        }
    }

    //패트롤 좌표 구하기
    public IBTNode.EBTNodeState RandomPatrolOnUpdate()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;
        if (_findTarget != null) return IBTNode.EBTNodeState.Fail;

        if(RandomPatrolEndPosition(transform.position, 10f, NavMesh.AllAreas) != Vector3.zero)
        {
            return IBTNode.EBTNodeState.Success;
        }

        return IBTNode.EBTNodeState.Running;
    }

    private Vector3 RandomPatrolEndPosition(Vector3 originPosition, float distance, int LayerMask)
    {
        Vector3 randomPoint = originPosition + Random.insideUnitSphere * distance;

        if(Physics.Raycast(randomPoint + Vector3.up * (distance + 1f), Vector3.down, out RaycastHit hitInfo, distance+5f, GroundLayer))
        {
            randomPoint.y = hitInfo.point.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, LayerMask))
            {
                PatrolPoint = hit.position;
                return PatrolPoint;
            }
        }

        return Vector3.zero;
    }

    public IBTNode.EBTNodeState PatrolMoveOnUpdate()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;
        if (_findTarget != null) return IBTNode.EBTNodeState.Fail;

        agent.stoppingDistance = 0.1f;

        MoveToTarget(PatrolPoint);

        float distance = agent.remainingDistance;

        if(distance > 0.1f)
        {
            animator.SetBool(HashMove, true);
            return IBTNode.EBTNodeState.Running;
        }

        animator.SetBool(HashMove, false);
        return IBTNode.EBTNodeState.Success;
    }
    #endregion
    #region Hurt
    public void HurtDamage(float damage, Transform attacker)
    {
        MonsterHP -= damage;

        if(MonsterHP <= 0)
        {
            isDie = true;
        }
        else
        {
            isHurt = true;
            _findTarget = attacker;            
        }
    }

    public IBTNode.EBTNodeState StartHurtAnimationOnUpdate()
    {
        if (isDie) return IBTNode.EBTNodeState.Fail;

        if (isHurt)
        {
            animator.SetTrigger(HashHurt);

            if(animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt"))
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
        if (isDead) return IBTNode.EBTNodeState.Success;

        if(isDie)
        {
            animator.SetTrigger(HashDie);
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
            StartCoroutine(DeadMonsterDestroy());

            return IBTNode.EBTNodeState.Success;
        }
        else
        return IBTNode.EBTNodeState.Running;
    }
    #endregion   
    IEnumerator DeadMonsterDestroy()
    {
        if(isDestroy) yield break;

        isDestroy = true;
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        isDestroy = false;
        yield break;
    }
}
