using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterView : MonoBehaviour
{
    MonsterBTRunner _monsterBTRunner;

    private void Awake()
    {
        _monsterBTRunner = new MonsterBTRunner(SetMonsterBT());
    }

    private void Update()
    {
        _monsterBTRunner.Execute();
    }

    IBTNode SetMonsterBT()
    {
        var followNodeList = new List<IBTNode>();
        followNodeList.Add(new MonsterActionNode());    //플레이어를 따라가는 액션
        followNodeList.Add(new MonsterActionNode());    //플레이어가 공격 사거리에 들어왔는지 확인하는 액션
        followNodeList.Add(new MonsterActionNode());    //몬스터가 따라가는 것을 멈출것인지 말것인지 결정하는 액션

        //Follow  상태일때, 순차적으로 실행할 이벤트 리스트들을 담는 시퀀스 노드 
        var followSeqNode = new MonsterSequenceNode(followNodeList);
        
        var AttackNodeList = new List<IBTNode>();
        AttackNodeList.Add(new MonsterActionNode());    //플레이어가 공격 사거리에 있는지 확인하는 액션
        AttackNodeList.Add(new MonsterActionNode());    //남은 공격 딜레이가 0인지 확인하는 액션
        AttackNodeList.Add(new MonsterActionNode());    //플레이어를 공격하는 액션

        var AttackSeqNode = new MonsterSequenceNode(AttackNodeList);

        List<IBTNode> rootSelectNode = new List<IBTNode>();
        rootSelectNode.Add(followSeqNode);
        rootSelectNode.Add(AttackSeqNode);

        var rootNode = new MonsterSelectorNode(rootSelectNode);
        return rootNode;
    }
}
