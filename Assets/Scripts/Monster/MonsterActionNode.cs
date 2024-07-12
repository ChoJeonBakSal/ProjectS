using System;

public class MonsterActionNode : IBTNode
{
    Func<IBTNode.EBTNodeState> _onUpdate = null;

    public MonsterActionNode(Func<IBTNode.EBTNodeState> onUpdate)
    {
        _onUpdate = onUpdate;
    }

    public IBTNode.EBTNodeState Evaluate()
    {
        if(_onUpdate != null) return _onUpdate.Invoke();
        else return IBTNode.EBTNodeState.Fail;
    }
}
