using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MonsterView : MonoBehaviour
{
    [Header("Monster_Info")]
    [SerializeField] private float MonsterAttakRange;
    public float AttackRange { get { return MonsterAttakRange; } }
    [SerializeField] private float MonsterAttackDelay;
    [SerializeField] private float MonsterPatrolDelayTimeAverage;
    [SerializeField] private float MonsterHP;
    [SerializeField] private float MonsterATK;
    public float AttackDamage {  get { return MonsterATK; } }
        
    [SerializeField] public Transform _findTarget { get; private set; }
    [SerializeField] LayerMask GroundLayer;

    MonsterBTRunner _monsterBTRunner;
    MonsterDetectZone detectZone;

    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;
    private Collider monsterCollider;

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

    private float defaultSpeed;
    private float defaultAngularSpeed;

    private void Awake()
    {
        _monsterBTRunner = new MonsterBTRunner(SetMonsterBT());
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        monsterCollider = GetComponent<Collider>();

        agent.speed = 1.5f;

        defaultSpeed = agent.speed;
        defaultAngularSpeed = agent.angularSpeed;
    }

    private void OnEnable()
    {
        monsterCollider.enabled = true;
        detectZone = GetComponentInChildren<MonsterDetectZone>();
        isAttacking = false;
        isHurt = false;
        isDie = false;
        isDead = false;
        isDestroy = false;

        agent.speed = defaultSpeed;
        agent.angularSpeed = defaultAngularSpeed;

        rb.isKinematic = false;        

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
        if (isAttacking || isHurt || isDie) return;

        agent.SetDestination(targetTransform);        
    }

    //�̵��ϸ� Ÿ�ٱ��� ���� �Ÿ��� ������Ʈ
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

        //���� �ִϸ��̼� ����
        animator.SetBool(HashMove, false);

        if (_AttackDelayTimer >= MonsterAttackDelay)
        {
            animator.SetTrigger(HashAttack);
            isAttacking = true;
            _AttackDelayTimer = 0;
            return IBTNode.EBTNodeState.Success;
        } else return IBTNode.EBTNodeState.Running;          
    }

    //���� ������ �������� Ȯ��
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
        var currentAnimationStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (currentAnimationStateInfo.IsName(animationName))
        {
            float normalizedTime = currentAnimationStateInfo.normalizedTime;
            isRunning = normalizedTime >= 0 && normalizedTime < 1.0f;
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
            Debug.Log("Hurt ���� ��...");
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
