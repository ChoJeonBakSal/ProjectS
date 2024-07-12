public interface IBTNode
{
    public enum EBTNodeState
    {
        Success,
        Fail,

    }

    public EBTNodeState Evaluate();
}
