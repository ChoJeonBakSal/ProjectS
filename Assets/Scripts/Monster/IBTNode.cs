public interface IBTNode
{
    public enum EBTNodeState
    {
        Success,
        Fail,
        running
    }

    public EBTNodeState Evaluate();
}
