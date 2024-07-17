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

    //셀렉트 Running이면 Running 반환
    //Success이면 Success 반환
    //Fail이면 다음 자식 실행
    //모든 자식 노드가 Fail을 반환하면 그때서야 Fail을 반환
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
