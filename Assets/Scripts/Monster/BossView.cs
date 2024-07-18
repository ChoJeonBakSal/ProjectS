using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BossView : MonoBehaviour
{
    [SerializeField] private float AttackRange;
    [SerializeField] private float AttackDelay;

    MonsterBTRunner _monsterBTRunner;

    private Transform _players;
    private NavMeshAgent agent;

    private float _AttackDelayTimer;

    private void Awake()
    {
        _monsterBTRunner = new MonsterBTRunner(SetMonsterBT());
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        _AttackDelayTimer = 0;
    }

    IBTNode SetMonsterBT()
    {
        var IdleNodeList = new List<IBTNode>();
        IdleNodeList.Add(new MonsterActionNode());

        List<IBTNode> rootSelectNode = new List<IBTNode>();

        var rootNode = new MonsterSelectorNode(rootSelectNode);
        return rootNode;
    }
    private void MoveToTarget(Vector3 targetTransform)
    {
        agent.SetDestination(targetTransform);
    }

    IBTNode.EBTNodeState CheckFollowingRangeOnUpdate()
    {
        float distance = Vector3.Distance(transform.position, _players.position);

        _AttackDelayTimer = Mathf.Clamp(_AttackDelayTimer + Time.deltaTime, 0f, AttackDelay);

        if (distance > AttackRange)
        {
            agent.stoppingDistance = AttackRange;
            //이동 애니메이션
            MoveToTarget(_players.position);
        }

        if (_AttackDelayTimer >= AttackDelay)
        {
            _AttackDelayTimer = 0;
            return IBTNode.EBTNodeState.Success;
        }
        else return IBTNode.EBTNodeState.Running;
    }
}
