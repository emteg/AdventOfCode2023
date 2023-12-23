namespace ConsoleApp19;

internal abstract record Rule
{
    public abstract bool Apply(Part part);

    public static Rule FromString(string str, Action<string, Part> enqueueWorkflowCallback)
    {
        if (str == "A")
            return new AcceptRule();
        if (str == "R")
            return new RejectRule();
        if (!str.Contains(':'))
            return new ForwardRule(str, enqueueWorkflowCallback);

        PartRating ratingToCompare = Enum.Parse<PartRating>(str.Substring(0, 1), true);
        bool acceptWhenLessThan = str[1] == '<';
        string[] parts = str.Split(':');
        uint threshold = uint.Parse(parts[0].Substring(2));
        string forwardWorkflowName = parts[1];

        return forwardWorkflowName switch
        {
            "A" => new CompareAndAcceptRule(ratingToCompare, acceptWhenLessThan, threshold),
            "R" => new CompareAndRejectRule(ratingToCompare, acceptWhenLessThan, threshold),
            _ => new CompareAndForwardRule(ratingToCompare, acceptWhenLessThan, threshold, forwardWorkflowName,
                enqueueWorkflowCallback)
        };
    }
}

internal sealed record AcceptRule : Rule
{
    public override bool Apply(Part part)
    {
        part.Accept();
        return true;
    }
    
    public override string ToString() => "A";
}

internal sealed record RejectRule : Rule
{
    public override bool Apply(Part part)
    {
        part.Reject();
        return true;
    }
    
    public override string ToString() => "R";
}