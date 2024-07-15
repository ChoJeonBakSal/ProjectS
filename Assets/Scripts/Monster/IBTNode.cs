public interface IBTNode
{
    public enum EBTNodeState
    {
        Success,
        Fail,
        Running
    }

    public EBTNodeState Evaluate();
}
