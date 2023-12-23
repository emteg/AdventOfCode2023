namespace ConsoleApp19;

internal sealed record CompareAndForwardRule : ComparePartRatingRule
{
    public string ForwardWorkflowName { get; }
    
    public CompareAndForwardRule(
        PartRating ratingToCompare, bool acceptWhenLessThan, uint threshold, string forwardWorkflowName, 
        Action<string, Part> enqueueWorkflowCallback) : base(ratingToCompare, acceptWhenLessThan, threshold)
    {
        ForwardWorkflowName = forwardWorkflowName;
        this.enqueueWorkflowCallback = enqueueWorkflowCallback;
    }
    
    public override string ToString()
    {
        return $"{RatingToCompare}{(AcceptWhenLessThan ? "<" : ">")}{Threshold}:{ForwardWorkflowName}";
    }

    protected override void PartMatches(Part part)
    {
        enqueueWorkflowCallback(ForwardWorkflowName, part);
    }
    
    private readonly Action<string, Part> enqueueWorkflowCallback;
}