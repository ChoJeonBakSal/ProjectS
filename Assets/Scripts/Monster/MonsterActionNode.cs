using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterActionNode : MonoBehaviour
{
    Func<IBTNode.EBTNodeState> _onUpdate = null;

    public MonsterActionNode(Func<IBTNode.EBTNodeState> onUpdate)
    {
        _onUpdate = onUpdate;
    }

}
