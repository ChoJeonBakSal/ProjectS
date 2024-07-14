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

    [SerializeField] Transform _findTarget;
    [SerializeField] LayerMask GroundLayer;

    MonsterBTRunner _monsterBTRunner;
    MonsterDetectZone detectZone;

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 PatrolPoint;

    private readonly int HashAttack = Animator.StringToHash("Attack");
    private readonly int HashHurt = Animator.StringToHash("Hit");
    private readonly int HashDie = Animator.StringToHash("Die");

    private float _timer = 0;
    private float _monsterPatrolDelay;
    private bool isHurt;
    private bool isDie;

    private void Awake()
    {
        _monsterBTRunner = new MonsterBTRunner(SetMonsterBT());
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        detectZone = GetComponentInChildren<MonsterDetectZone>();
        isHurt = false;
        isDie = false;

        if (detectZone!= null)
        {
            detectZone.OnTargetChanged += SetTarget;
        }

        _monsterPatrolDelay = Random.Range(MonsterPatrolDelayTimeAverage - 1, MonsterPatrolDelayTimeAverage + 1);
    }

    private void OnDisable()
    {
        if(detectZone != null)
        {
            detectZone.OnTargetChanged -= SetTarget;
        }
    }

    private void SetTarget(Transform target)
    {
        _findTarget = target;
    }

    private void Update()
    {
        _monsterBTRunner.Execute();
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
        agent.SetDestination(targetTransform);
        //플레이어가 해당 방향으로 rotation 하도록 설정
    }

    //이동하며 타겟까지 남은 거리를 업데이트
    public IBTNode.EBTNodeState CheckFollowingRangeOnUpdate()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;

        float distance = Vector3.Distance(_findTarget.position, transform.position);

        if(distance > MonsterAttakRange)
        {
            agent.stoppingDistance = MonsterAttakRange;

            MoveToTarget(_findTarget.position);
            return IBTNode.EBTNodeState.Running;
        }

        //공격 애니메이션 실행
        animator.SetTrigger(HashAttack);
        return IBTNode.EBTNodeState.Success;
    }

    //공격 동작이 끝났는지 확인
    public IBTNode.EBTNodeState CompleteFollowOnUpdate()
    {
        if (isDie || isHurt) return IBTNode.EBTNodeState.Fail;

        if (IsAnimationRunning(HashAttack))
        {
            return IBTNode.EBTNodeState.Running;
        }

        //animator.CrossFade(HashAttack, MonsterAttackDelay);
        return IBTNode.EBTNodeState.Success;
    }

    private bool IsAnimationRunning(int animationHash)
    {
        if(animator == null) return false;

        bool isRunning = false;
        if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(animationHash))
        {
            float normalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            isRunning = normalizedTime != 0 && normalizedTime < 1.0f;
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

        if (_timer < _monsterPatrolDelay)
        {
            _timer += Time.deltaTime;
            return IBTNode.EBTNodeState.Running;
        }
        else
        {
            _timer = 0f;
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
            return IBTNode.EBTNodeState.Running;
        }

        return IBTNode.EBTNodeState.Success;
    }
    #endregion
    #region Hurt
    public void HurtDamage(float damage)
    {
        MonsterHP -= damage;

        if(MonsterHP <= 0)
        {
            isDie = true;
        }
        else
        {
            isHurt = true;
            animator.SetTrigger(HashHurt);
        }
    }

    public IBTNode.EBTNodeState StartHurtAnimationOnUpdate()
    {
        if (isDie) return IBTNode.EBTNodeState.Fail;

        if (isHurt)
        {
            animator.SetTrigger(HashHurt);
            return IBTNode.EBTNodeState.Success;
        }

        return IBTNode.EBTNodeState.Fail;
    }

    public IBTNode.EBTNodeState CheckRunningHurtAnimationOnUpdate()
    {
        if (isDie) return IBTNode.EBTNodeState.Fail;

        if (IsAnimationRunning(HashHurt))
        {
            return IBTNode.EBTNodeState.Running;
        }

        isHurt = false;
        return IBTNode.EBTNodeState.Success;
    }
    #endregion
    #region Die
    public IBTNode.EBTNodeState CheckHPZeroOnUpdate()
    {
        if(isDie)
        {
            animator.SetTrigger(HashDie);
            return IBTNode.EBTNodeState.Success;
        }

        return IBTNode.EBTNodeState.Fail;
    }

    public IBTNode.EBTNodeState CheckRunningDieAnimationOnUpdate()
    {
        if (IsAnimationRunning(HashDie))
        {
            return IBTNode.EBTNodeState.Running;
        }

        Destroy(gameObject);
        return IBTNode.EBTNodeState.Success;
    }
    #endregion
}
