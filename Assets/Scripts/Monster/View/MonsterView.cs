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
        followNodeList.Add(new MonsterActionNode());    //�÷��̾ ���󰡴� �׼�
        followNodeList.Add(new MonsterActionNode());    //�÷��̾ ���� ��Ÿ��� ���Դ��� Ȯ���ϴ� �׼�
        followNodeList.Add(new MonsterActionNode());    //���Ͱ� ���󰡴� ���� ��������� �������� �����ϴ� �׼�

        //Follow  �����϶�, ���������� ������ �̺�Ʈ ����Ʈ���� ��� ������ ��� 
        var followSeqNode = new MonsterSequenceNode(followNodeList);
        
        var AttackNodeList = new List<IBTNode>();
        AttackNodeList.Add(new MonsterActionNode());    //�÷��̾ ���� ��Ÿ��� �ִ��� Ȯ���ϴ� �׼�
        AttackNodeList.Add(new MonsterActionNode());    //���� ���� �����̰� 0���� Ȯ���ϴ� �׼�
        AttackNodeList.Add(new MonsterActionNode());    //�÷��̾ �����ϴ� �׼�

        var AttackSeqNode = new MonsterSequenceNode(AttackNodeList);

        List<IBTNode> rootSelectNode = new List<IBTNode>();
        rootSelectNode.Add(followSeqNode);
        rootSelectNode.Add(AttackSeqNode);

        var rootNode = new MonsterSelectorNode(rootSelectNode);
        return rootNode;
    }
}
