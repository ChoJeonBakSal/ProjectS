using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSequenceNode : IBTNode
{
    List<IBTNode> _childList;
    int _currentChild;

    public MonsterSequenceNode(List<IBTNode> childList)
    {
        _childList = childList;
        _currentChild = 0;
    }

    //시퀀스 노드는 Reunning이면, 상태를 유지
    //Success면 다음 자식으로 이동, Fail이면 Fail을 반환해야 한다.
    public IBTNode.EBTNodeState Evaluate()
    {
        if(_childList == null || _childList.Count == 0) return IBTNode.EBTNodeState.Fail;

        for(;  _currentChild < _childList.Count; _currentChild++)
        {
            var childState = _childList[_currentChild].Evaluate();

            switch(childState)
            {
                case IBTNode.EBTNodeState.Running:
                    return IBTNode.EBTNodeState.Running;
                case IBTNode.EBTNodeState.Success:
                    continue;
                case IBTNode.EBTNodeState.Fail:
                    _currentChild = 0;
                    return IBTNode.EBTNodeState.Fail;
            }
        }

        _currentChild = 0;
        return IBTNode.EBTNodeState.Success;
    }
}
