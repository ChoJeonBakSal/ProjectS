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
        IdleNodeList.Add(new MonsterActionNode(WaitPatrolDelay));      //��Ʈ���� ������ ��ٸ���
        IdleNodeList.Add(new MonsterActionNode(RandomPatrolOnUpdate));      //��Ʈ���� ��ġ �ޱ�
        IdleNodeList.Add(new MonsterActionNode(PatrolMoveOnUpdate));    //������ ��ġ���� ��Ʈ�� �̵�

        var IdleSeqNode = new MonsterSequenceNode(IdleNodeList);

        var followNodeList = new List<IBTNode>();
        followNodeList.Add(new MonsterActionNode(CheckFollowingRangeOnUpdate));    //�÷��̾� ���󰡱�
        followNodeList.Add(new MonsterActionNode(CompleteFollowOnUpdate));    //�÷��̾ ���� ��Ÿ��� ���Դ��� �Ǵ�

        //Follow  �����϶�, ���������� ������ �̺�Ʈ ����Ʈ���� ��� ������ ��� 
        var followSeqNode = new MonsterSequenceNode(followNodeList);

        var HurtNodeList = new List<IBTNode>();
        HurtNodeList.Add(new MonsterActionNode(StartHurtAnimationOnUpdate));  //Hurt �ִϸ��̼� ����
        HurtNodeList.Add(new MonsterActionNode(CheckRunningHurtAnimationOnUpdate));  //Hurt �ִϸ��̼��� ����ǰ� �ִ��� Ȯ��

        var HurtSeqNode = new MonsterSequenceNode(HurtNodeList);

        var DeadNodeList = new List<IBTNode>();
        DeadNodeList.Add(new MonsterActionNode(CheckHPZeroOnUpdate));  // HP <= 0 ���� Ȯ��
        DeadNodeList.Add(new MonsterActionNode(CheckRunningDieAnimationOnUpdate));  // Monster ��� �ִϸ��̼��� ������ Distory

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

    //Ÿ���� ���� �̵�
    private void MoveToTarget(Vector3 targetTransform)
    {
        agent.SetDestination(targetTransform);
        //�÷��̾ �ش� �������� rotation �ϵ��� ����
    }

    //�̵��ϸ� Ÿ�ٱ��� ���� �Ÿ��� ������Ʈ
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

        //���� �ִϸ��̼� ����
        animator.SetTrigger(HashAttack);
        return IBTNode.EBTNodeState.Success;
    }

    //���� ������ �������� Ȯ��
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

    //��Ʈ�� ������ ��ٸ���
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

    //��Ʈ�� ��ǥ ���ϱ�
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
