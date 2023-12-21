// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        
        TextReader test = new StringReader(@"2413
3215
3255
3446");

        TextReader sample = new StringReader(@"2413432311323
3215453535623
3255245654254
3446585845452
4546657867536
1438598798454
4457876987766
3637877979653
4654967986887
4564679986453
1224686865563
2546548887735
4322674655533");

        uint[][] grid = ReadGrid(sample.EnumerateLines());
        (uint x, uint y) goal = ((uint)grid[0].Length - 1, (uint)grid.Length - 1);
        Step firstStep = new(0, 0, Direction.Right);
        Dictionary<uint, List<Step>> steps = new();
        steps.Add(firstStep.HeatLoss, new List<Step> {firstStep});
        Step? bestStepAtGoal = null;
        int counter = 0;
        
        while (steps.Count != 0)
        {
            counter++;
            Step step = BestStep(steps);
            if (counter % 20000 == 0)
            {
                Console.WriteLine($"Best step is at {step.X}|{step.Y} @ {step.HeatLoss} after {step.Length} steps.");
                PrintPath(step, grid);
            }
            List<Step> nextSteps = PossibleNextStepsOf(step, grid)
                .Where(stp => stp.StepsInDirectionSoFar <= 3)
                .ToList();
            steps[step.HeatLoss].Remove(step);
            if (steps[step.HeatLoss].Count == 0)
                steps.Remove(step.HeatLoss);
            foreach (Step stepAtGoal in nextSteps.Where(stp => stp.X == goal.x && stp.Y == goal.y))
            {
                Console.WriteLine($"Found a path to goal with a total heat loss of {stepAtGoal.HeatLoss}");
                bestStepAtGoal = stepAtGoal;
                break;
            }

            if (bestStepAtGoal is not null)
                break;
            
            foreach (Step nextStep in nextSteps)
            {
                if (steps.ContainsKey(nextStep.HeatLoss))
                    steps[nextStep.HeatLoss].Add(nextStep);
                else
                    steps.Add(nextStep.HeatLoss, new List<Step> {nextStep});
            }
        }
        
        steps.Clear();

        PrintPath(bestStepAtGoal!, grid);
    }

    private static void PrintPath(Step bestStepAtGoal, uint[][] grid)
    {
        List<Step> path = new();
        Step start = bestStepAtGoal;
        while (start.Previous is not null)
        {
            path.Add(start);
            start = start.Previous;
        }
        path.Add(start);
        
        PrintPath(path, grid);
    }

    private static void PrintPath(List<Step> path, uint[][] grid)
    {
        for (var y = 0; y < grid.Length; y++)
        {
            for (var x = 0; x < grid[y].Length; x++)
            {
                Step? step = path.FirstOrDefault(it => it.X == x && it.Y == y);
                if (step is not null) 
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(grid[y][x]);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }

    private static IEnumerable<Step> PossibleNextStepsOf(Step step, uint[][] grid)
    {
        if (step.X > 0 && !step.HasPassedThrough(step.X -1, step.Y))
            yield return new Step(
                step.X -1,
                step.Y, 
                Direction.Left,
                step, 
                grid[step.Y][step.X -1]);
        if (step.X < grid[0].Length - 1 && !step.HasPassedThrough(step.X +1, step.Y))
            yield return new Step(
                step.X +1,
                step.Y,
                Direction.Right,
                step,
                grid[step.Y][step.X +1]);
        if (step.Y > 0 && !step.HasPassedThrough(step.X, step.Y -1))
            yield return new Step(
                step.X,
                step.Y -1, 
                Direction.Up,
                step, 
                grid[step.Y -1][step.X]);
        if (step.Y < grid.Length - 1 && !step.HasPassedThrough(step.X, step.Y +1))
            yield return new Step(
                step.X,
                step.Y +1,
                Direction.Down,
                step,
                grid[step.Y +1][step.X]);
    }

    private static Step BestStep(Dictionary<uint, List<Step>> steps)
    {
        return steps[steps.Keys.Min()].First();

        //uint bestHeatLoss = uint.MaxValue;
        //Step? bestStep = null;
        //
        //foreach (Step step in steps)
        //{
        //    if (step.HeatLoss < bestHeatLoss)
        //    {
        //        bestHeatLoss = step.HeatLoss;
        //        bestStep = step;
        //    }
        //}
        //
        //return bestStep!;
    }

    private static uint[][] ReadGrid(IEnumerable<string> input)
    {
        return input
            .Select(line => line.ToCharArray())
            .Select(chars => chars
                .Select(c => uint.Parse(c.ToString()))
                .ToArray())
            .ToArray();
    }
}

[DebuggerDisplay("{X}|{Y} {HeatLoss} ({Length})")]
internal sealed class Step
{
    public readonly int X;
    public readonly int Y;
    public readonly Step? Previous = null;
    public readonly Direction IncomingDirection;
    public readonly uint StepsInDirectionSoFar = 1;
    public readonly uint HeatLoss = 0;
    public readonly uint Length = 1;

    public Step(int x, int y, Direction incomingDirection)
    {
        X = x;
        Y = y;
        IncomingDirection = incomingDirection;
    }

    public Step(int x, int y, Direction incomingDirection, Step previous, uint heatLoss) : this(x, y, incomingDirection)
    {
        Previous = previous;
        if (incomingDirection == previous.IncomingDirection)
            StepsInDirectionSoFar = previous.StepsInDirectionSoFar + 1;
        HeatLoss = previous.HeatLoss + heatLoss;
        Length = previous.Length + 1;
    }

    public bool HasPassedThrough(int x, int y)
    {
        if (X == x && Y == y)
            return true;
        
        if (Previous is null)
            return false;

        return Previous.HasPassedThrough(x, y);
    }
}
