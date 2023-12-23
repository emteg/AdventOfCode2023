// See https://aka.ms/new-console-template for more information

using ConsoleApp19;
using Util;

internal static class Program
{
    private static readonly Queue<(string workflowName, Part part)> QueuedParts = new();

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sample = @"px{a<2006:qkq,m>2090:A,rfg}
pv{a>1716:R,A}
lnx{m>1548:A,A}
rfg{s<537:gd,x>2440:R,A}
qs{s>3448:A,lnx}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
in{s<1351:px,qqz}
qqz{s>2770:qs,m<1801:hdj,R}
gd{a>3333:R,R}
hdj{m>838:A,pv}

{x=787,m=2655,a=1222,s=2876}
{x=1679,m=44,a=2067,s=496}
{x=2036,m=264,a=79,s=2244}
{x=2461,m=1339,a=466,s=291}
{x=2127,m=1623,a=2188,s=1013}";

        Part1(new StringReader(sample));
        Part1(File.OpenText("input.txt"));

        Part2(new StringReader(sample));
        Part2(File.OpenText("input.txt"));
    }
    
    private static void Part1(TextReader sample)
    {
        Dictionary<string, Workflow> workflows = new();
        List<Part> parts = ReadWorkflowsAndParts(sample.EnumerateLines(), workflows)
            .ToList();

        foreach (Part part in parts) 
            QueuedParts.Enqueue(("in", part));

        while (QueuedParts.TryDequeue(out (string workflowName, Part part) entry)) 
            workflows[entry.workflowName].Execute(entry.part);
        
        Console.WriteLine(parts
            .Where(part => part.Accepted)
            .Select(part => part.Sum())
            .Sum());
    }

    private static void Part2(TextReader input)
    {
        Dictionary<string, Workflow> workflows = new();

        List<Workflow> wfs = ReadWorkflows(input.EnumerateLines()).ToList();

        foreach (Workflow workflow in wfs)
            workflows.Add(workflow.Name, workflow);

        Queue<(RatingRange, string?)> queue = new();
        queue.Enqueue((new RatingRange(), "in"));

        List<RatingRange> acceptedRanges = new();
        while (queue.TryDequeue(out (RatingRange range, string? wfName) item))
        {
            if (item.wfName is null && item.range.IsAccepted)
                acceptedRanges.Add(item.range);
            else if (item.wfName is not null)
            {
                Workflow workflow = workflows[item.wfName];
                foreach ((RatingRange range, string? wfName) tuple in PassRatingThroughWorkflow(item.range, workflow))
                    queue.Enqueue((tuple.range, tuple.wfName));
            }
        }

        Console.WriteLine(acceptedRanges.Sum(r => r.Combinations()));
    }

    private static IEnumerable<(RatingRange, string?)> PassRatingThroughWorkflow(RatingRange rating, Workflow workflow)
    {
        foreach (Rule rule in workflow.Rules)
        {
            if (rule is CompareAndForwardRule cafRule)
            {
                (RatingRange left, rating) = rating.WithRange(cafRule.RatingToCompare, cafRule.AcceptWhenLessThan, cafRule.Threshold);
                left.RuleApplied(workflow, rule);
                rating.RuleApplied(workflow, workflow.Rules.IndexOf(rule));
                yield return (left, cafRule.ForwardWorkflowName);
                continue;
            }

            if (rule is ForwardRule fwdRule)
            {
                rating.RuleApplied(workflow, rule);
                yield return (rating, fwdRule.ForwardWorkflowName);
                continue;
            }

            if (rule is CompareAndAcceptRule caRule)
            {
                (RatingRange left, rating) = rating.WithRange(caRule.RatingToCompare, caRule.AcceptWhenLessThan, caRule.Threshold);
                left.RuleApplied(workflow, rule);
                rating.RuleApplied(workflow, workflow.Rules.IndexOf(rule));
                if (caRule.AcceptWhenLessThan && left.GetRange(caRule.RatingToCompare).End.Value < caRule.Threshold)
                    left.IsAccepted = true;
                else if (caRule.AcceptWhenLessThan && left.GetRange(caRule.RatingToCompare).Start.Value >= caRule.Threshold)
                    rating.IsAccepted = true;
                else if (!caRule.AcceptWhenLessThan &&
                         left.GetRange(caRule.RatingToCompare).Start.Value >= caRule.Threshold)
                    left.IsAccepted = true;
                else
                    rating.IsAccepted = true;
                yield return (left, null);
                continue;
            }

            if (rule is CompareAndRejectRule crRule)
            {
                (RatingRange left, rating) = rating.WithRange(crRule.RatingToCompare, crRule.AcceptWhenLessThan, crRule.Threshold);
                left.RuleApplied(workflow, rule);
                rating.RuleApplied(workflow, workflow.Rules.IndexOf(rule));
                yield return (left, null);
                continue;
            }

            if (rule is RejectRule)
            {
                rating.RuleApplied(workflow, rule);
                yield return (rating, null);
                continue;
            }

            if (rule is AcceptRule)
            {
                rating.IsAccepted = true;
                rating.RuleApplied(workflow, rule);
                yield return (rating, null);
                continue;
            }

            throw new InvalidOperationException();
        }
    }

    private static IEnumerable<Part> ReadWorkflowsAndParts(IEnumerable<string> lines, Dictionary<string, Workflow> workflows)
    {
        bool readWorkflows = true;
        foreach (string line in lines)
        {
            if (line == string.Empty)
            {
                readWorkflows = false;
                continue;
            }

            if (readWorkflows)
            {
                Workflow w = Workflow.FromString(line, EnqueueWorkflowCallback);
                workflows.Add(w.Name, w);
                continue;
            }

            yield return Part.FromString(line);
        }
    }
    
    private static IEnumerable<Workflow> ReadWorkflows(IEnumerable<string> lines) 
        => lines
            .TakeWhile(line => line != string.Empty)
            .Select(line => Workflow.FromString(line, EnqueueWorkflowCallback));

    private static void EnqueueWorkflowCallback(string workflowName, Part part)
    {
        QueuedParts.Enqueue((workflowName, part));
    }
}

internal enum PartRating
{
    X,
    M,
    A,
    S
}

internal sealed record RatingRange
{
    public Range X { get; private set; } = new(1, 4000);
    public Range M { get; private set; } = new(1, 4000);
    public Range A { get; private set; } = new(1, 4000);
    public Range S { get; private set; } = new(1, 4000);

    public bool IsAccepted = false;
    public string Workflows => string.Join(", ", workflows);
    public string Rules => string.Join(", ", rules);

    public (RatingRange, RatingRange) WithRange(PartRating rating, bool acceptWhenLessThan, uint threshold)
    {
        Range original = GetRange(rating);

        Range leftNewRange;
        Range rightNewRange;
        if (acceptWhenLessThan)
        {
            leftNewRange = new Range(original.Start.Value, (int)threshold -1);
            rightNewRange = new Range((int)threshold, original.End.Value);
        }
        else
        {
            leftNewRange = new Range((int)threshold +1, original.End.Value);
            rightNewRange = new Range(original.Start.Value, (int)threshold);
        }

        Range leftX = rating is PartRating.X ? leftNewRange : new Range(X.Start, X.End);
        Range leftM = rating is PartRating.M ? leftNewRange : new Range(M.Start, M.End);
        Range leftA = rating is PartRating.A ? leftNewRange : new Range(A.Start, A.End);
        Range leftS = rating is PartRating.S ? leftNewRange : new Range(S.Start, S.End);
        
        Range rightX = rating is PartRating.X ? rightNewRange : new Range(X.Start, X.End);
        Range rightM = rating is PartRating.M ? rightNewRange : new Range(M.Start, M.End);
        Range rightA = rating is PartRating.A ? rightNewRange : new Range(A.Start, A.End);
        Range rightS = rating is PartRating.S ? rightNewRange : new Range(S.Start, S.End);

        return (
            new RatingRange { X = leftX, M = leftM, A = leftA, S = leftS, workflows = new List<string>(workflows)}, 
            new RatingRange { X = rightX, M = rightM, A = rightA, S = rightS, workflows = new List<string>(workflows) });
    }

    public long Combinations()
    {
        long result = 1;
        foreach (PartRating rating in Enum.GetValues<PartRating>())
        {
            Range range = GetRange(rating);
            result *= (range.End.Value - range.Start.Value + 1);
        }
        return result;
    }

    public void RuleApplied(Workflow workflow, Rule rule)
    {
        workflows.Add(workflow.Name);
        rules.Add(rule.ToString());
    }
    
    public void RuleApplied(Workflow workflow, int ruleIndex)
    {
        workflows.Add(workflow.Name);
        rules.Add(ruleIndex.ToString());
    }

    public Range GetRange(PartRating rating)
    {
        return rating switch
        {
            PartRating.X => X,
            PartRating.M => M,
            PartRating.A => A,
            PartRating.S => S,
            _ => throw new InvalidOperationException()
        };
    }

    private List<string> workflows = new();
    private List<string> rules = new();
}

internal sealed record Workflow(string Name, List<Rule> Rules)
{
    public void Execute(Part part)
    {
        foreach (Rule rule in Rules)
        {
            if (rule.Apply(part))
                return;
        }

        throw new InvalidOperationException();
    }
    
    public void ReplaceRule(int index, Rule newRule)
    {
        Rules.RemoveAt(index);
        Rules.Insert(index, newRule);
    }

    public static Workflow FromString(string line, Action<string, Part> enqueueWorkflowCallback)
    {
        string name = line.Substring(0, line.IndexOf('{'));

        List<Rule> rules = line
            .Substring(name.Length +1, line.Length - name.Length -2)
            .Split(',')
            .Select(str => Rule.FromString(str, enqueueWorkflowCallback))
            .ToList();
        
        return new Workflow(name, rules);
    }
}