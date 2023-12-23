namespace ConsoleApp19;

internal sealed record ForwardRule : Rule
{
    public string ForwardWorkflowName { get; }

    public Action<string, Part> EnqueueWorkflowCallback { get; }

    public ForwardRule(string forwardWorkflowName, Action<string, Part> enqueueWorkflowCallback)
    {
        ForwardWorkflowName = forwardWorkflowName;
        EnqueueWorkflowCallback = enqueueWorkflowCallback;
    }

    public override bool Apply(Part part)
    {
        EnqueueWorkflowCallback(ForwardWorkflowName, part);
        return true;
    }
}