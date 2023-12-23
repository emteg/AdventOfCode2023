// See https://aka.ms/new-console-template for more information

using ConsoleApp19;
using Util;

internal static class Program
{
    private static Dictionary<string, Workflow> workflows = new();
    private static Queue<(string workflowName, Part part)> queuedParts = new();
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader test = new StringReader(@"in{s<1351:px,qqz}
px{a<2006:qkq,m>2090:A,rfg}
qkq{x<1416:A,crn}
crn{x>2662:A,R}
rfg{s<537:R,x>2440:R,A}
qqz{s>2770:A,m<1801:hdj,R}
hdj{m>838:A,pv}
pv{a>1716:R,A}");

        TextReader sample = new StringReader(@"px{a<2006:qkq,m>2090:A,rfg}
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
{x=2127,m=1623,a=2188,s=1013}");

        //Part1(sample);
        //Part1(File.OpenText("input.txt"));

        List<Workflow> wfs = ReadWorkflows(test.EnumerateLines()).ToList();
        SimplifyWorkflows(wfs);
        workflows.Clear();
        
        foreach (Workflow workflow in wfs) 
            workflows.Add(workflow.Name, workflow);

        Queue<(RatingRange, string?)> queue = new();
        queue.Enqueue((new RatingRange(), "in"));

        List<RatingRange> finishedRanges = new();
        while (queue.TryDequeue(out (RatingRange range, string? wfName) item))
        {
            if (item.wfName is null && item.range.IsAccepted)
                finishedRanges.Add(item.range);
            else if (item.wfName is not null)
            {
                Workflow workflow = workflows[item.wfName];
                foreach ((RatingRange range, string? wfName) tuple in PassRatingThroughWorkflow(item.range, workflow))
                    queue.Enqueue((tuple.range, tuple.wfName));
            }
        }
    }
    
    private static IEnumerable<(RatingRange, string?)> PassRatingThroughWorkflow(RatingRange rating, Workflow workflow)
    {
        foreach (Rule rule in workflow.Rules)
        {
            if (rule is CompareAndForwardRule cafRule)
            {
                (RatingRange left, rating) = rating.WithRange(cafRule.RatingToCompare, cafRule.AcceptWhenLessThan, cafRule.Threshold);
                yield return (left, cafRule.ForwardWorkflowName);
                continue;
            }

            if (rule is ForwardRule fwdRule)
            {
                yield return (rating, fwdRule.ForwardWorkflowName);
                continue;
            }

            if (rule is CompareAndAcceptRule caRule)
            {
                (RatingRange left, rating) = rating.WithRange(caRule.RatingToCompare, caRule.AcceptWhenLessThan, caRule.Threshold);
                if (caRule.AcceptWhenLessThan)
                    rating.IsAccepted = true;
                else
                    left.IsAccepted = true;
                yield return (left, null);
                continue;
            }

            if (rule is CompareAndRejectRule crRule)
            {
                (RatingRange left, rating) = rating.WithRange(crRule.RatingToCompare, crRule.AcceptWhenLessThan, crRule.Threshold);
                yield return (left, null);
                continue;
            }

            if (rule is RejectRule)
            {
                yield return (rating, null);
                continue;
            }

            if (rule is AcceptRule)
            {
                rating.IsAccepted = true;
                yield return (rating, null);
                continue;
            }

            throw new NotImplementedException();
        }
    }

    private static void Part1(TextReader sample)
    {
        workflows.Clear();
        queuedParts.Clear();
        List<Part> parts = ReadWorkflowsAndParts(sample.EnumerateLines())
            .ToList();

        // not very efficient...
        List<Workflow> wfs = workflows.Values.ToList();
        SimplifyWorkflows(wfs);
        workflows.Clear();
        foreach (Workflow workflow in wfs) 
            workflows.Add(workflow.Name, workflow);
        
        foreach (Part part in parts) 
            queuedParts.Enqueue(("in", part));

        while (queuedParts.TryDequeue(out (string workflowName, Part part) entry)) 
            workflows[entry.workflowName].Execute(entry.part);
        
        Console.WriteLine(parts
            .Where(part => part.Accepted)
            .Select(part => part.Sum())
            .Sum());
    }

    private static void SimplifyWorkflows(List<Workflow> wfs)
    {
        while (true)
        {
            bool changeWasMade = false;

            for (int i = wfs.Count - 1; i >= 0; i--)
            {
                if (wfs[i].Rules.All(rule => rule is AcceptRule or CompareAndAcceptRule))
                {
                    RemoveAcceptingWorkflow(wfs, i);
                    changeWasMade = true;
                    continue;
                }

                if (wfs[i].Rules.All(rule => rule is RejectRule or CompareAndRejectRule))
                {
                    RemoveRejectingWorkflow(wfs, i);
                    changeWasMade = true;
                    continue;
                }
            }

            if (!changeWasMade)
                break;
        }
    }

    private static void RemoveAcceptingWorkflow(List<Workflow> wfs, int index)
    {
        Workflow workflow = wfs[index];

        Console.WriteLine($"Workflow {workflow.Name} accepts everything.");

        List<Workflow> workflowsToUpdate = wfs
            .Where(it => it.Rules.Any(rule => IsForwardOrCompareAndForwardRule(rule, workflow.Name)))
            .ToList();
                    
        foreach (Workflow workflowToUpdate in workflowsToUpdate)
        {
            Console.WriteLine($"\tUpdating {workflowToUpdate.Name} to not forwarding to {workflow.Name}");
            for (int j = 0; j < workflowToUpdate.Rules.Count; j++)
            {
                if (IsForwardRule(workflowToUpdate.Rules[j], workflow.Name))
                {
                    Console.WriteLine($"\tReplacing ForwardRule {j} with AcceptRule");
                    workflowToUpdate.ReplaceRule(j, new AcceptRule());
                }
                else if (IsCompareAndForwardRule(workflowToUpdate.Rules[j], workflow.Name))
                {
                    CompareAndForwardRule r = (CompareAndForwardRule)workflowToUpdate.Rules[j];
                    Console.WriteLine($"\tReplacing CompareAndForwardRule {j} with CompareAndAcceptRule");
                    workflowToUpdate.ReplaceRule(j, new CompareAndAcceptRule(r.RatingToCompare, r.AcceptWhenLessThan, r.Threshold));
                }
            }
        }
                    
        wfs.RemoveAt(index);
        Console.WriteLine("\tWorkflow deleted.");
    }

    private static void RemoveRejectingWorkflow(List<Workflow> wfs, int index)
    {
        Workflow workflow = wfs[index];

        Console.WriteLine($"Workflow {workflow.Name} rejects everything.");

        List<Workflow> workflowsToUpdate = wfs
            .Where(it => it.Rules.Any(rule => IsForwardOrCompareAndForwardRule(rule, workflow.Name)))
            .ToList();
                    
        foreach (Workflow workflowToUpdate in workflowsToUpdate)
        {
            Console.WriteLine($"\tUpdating {workflowToUpdate.Name} to not forwarding to {workflow.Name}");
            for (int j = 0; j < workflowToUpdate.Rules.Count; j++)
            {
                if (IsForwardRule(workflowToUpdate.Rules[j], workflow.Name))
                {
                    Console.WriteLine($"\tReplacing ForwardRule {j} with RejectRule");
                    workflowToUpdate.ReplaceRule(j, new RejectRule());
                }
                else if (IsCompareAndForwardRule(workflowToUpdate.Rules[j], workflow.Name))
                {
                    CompareAndForwardRule r = (CompareAndForwardRule)workflowToUpdate.Rules[j];
                    Console.WriteLine($"\tReplacing CompareAndForwardRule {j} with CompareAndRejectRule");
                    workflowToUpdate.ReplaceRule(j, new CompareAndRejectRule(r.RatingToCompare, r.AcceptWhenLessThan, r.Threshold));
                }
            }
        }
                    
        wfs.RemoveAt(index);
        Console.WriteLine("\tWorkflow deleted.");
    }

    private static bool IsForwardOrCompareAndForwardRule(Rule rule, string name)
    {
        if (IsForwardRule(rule, name))
            return true;

        if (IsCompareAndForwardRule(rule, name))
            return true;

        return false;
    }

    private static bool IsCompareAndForwardRule(Rule rule, string name)
    {
        return rule is CompareAndForwardRule compRule && compRule.ForwardWorkflowName == name;
    }

    private static bool IsForwardRule(Rule rule, string name)
    {
        return rule is ForwardRule fwdRule && fwdRule.ForwardWorkflowName == name;
    }

    private static IEnumerable<Part> ReadWorkflowsAndParts(IEnumerable<string> lines)
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
        queuedParts.Enqueue((workflowName, part));
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
            new RatingRange { X = leftX, M = leftM, A = leftA, S = leftS }, 
            new RatingRange { X = rightX, M = rightM, A = rightA, S = rightS });
    }

    private Range GetRange(PartRating rating)
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

    private void SetRange(PartRating rating, int start, int end)
    {
        Range range = new(start, end);
        
        if (rating is PartRating.X)
            X = range;
        else if (rating is PartRating.M)
            M = range;
        else if (rating is PartRating.A)
            A = range;
        else
            S = range;
    }
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