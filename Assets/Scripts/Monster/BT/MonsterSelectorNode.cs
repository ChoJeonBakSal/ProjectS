using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MonsterSelectorNode : IBTNode
{
    List<IBTNode> _childNodeList;

    public MonsterSelectorNode(List<IBTNode> childNodeList) 
    {
        _childNodeList = childNodeList;
    }

    //����Ʈ Running�̸� Running ��ȯ
    //Success�̸� Success ��ȯ
    //Fail�̸� ���� �ڽ� ����
    //��� �ڽ� ��尡 Fail�� ��ȯ�ϸ� �׶����� Fail�� ��ȯ
    public IBTNode.EBTNodeState Evaluate()
    {
        if( _childNodeList == null ) return IBTNode.EBTNodeState.Fail;

        foreach (var child in _childNodeList)
        {
            var childState = child.Evaluate();
            switch (childState)
            {
                case IBTNode.EBTNodeState.Running:
                    return IBTNode.EBTNodeState.Running;
                case IBTNode.EBTNodeState.Success:
                    return IBTNode.EBTNodeState.Success;
            }
        }

        return IBTNode.EBTNodeState.Fail ;
    }
}
