namespace ConsoleApp19;

internal sealed record ForwardRule(string ForwardWorkflowName, Action<string, Part> EnqueueWorkflowCallback) : Rule
{
    public override bool Apply(Part part)
    {
        EnqueueWorkflowCallback(ForwardWorkflowName, part);
        return true;
    }
    public override string ToString() => ForwardWorkflowName;
}