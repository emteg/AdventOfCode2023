using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        StringReader sampleInput = new(@"0 3 6 9 12 15
1 3 6 10 15 21
10 13 16 21 30 45");

        Part1(sampleInput.EnumerateLines(), true);
        Part1(File.OpenText("input.txt").EnumerateLines(), true);
        
        Part1(sampleInput.EnumerateLines(), false);
        Part1(File.OpenText("input.txt").EnumerateLines(), false);
    }

    private static void Part1(IEnumerable<string> lines, bool forward)
    {
        int result = lines
            .Select(line => line
                .Split(' ')
                .Select(int.Parse)
                .ToArray())
            .Select(numbers => Predict(numbers, forward))
            .Sum();

        Console.WriteLine(result);
    }

    private static int Predict(IReadOnlyList<int> numbers, bool forward)
    {
        bool diffsAreAllZero = true;
        int[] diffs = new int[numbers.Count - 1];
        for (int i = 0; i < numbers.Count - 1; i++)
        {
            int diff = numbers[i + 1] -  numbers[i];
            if (diff != 0)
                diffsAreAllZero = false;
            diffs[i] = diff;
        }

        return (diffsAreAllZero, forward) switch
        {
            (true, true) => numbers[^1],
            (true, false) => numbers[0],
            (false, true) => numbers[^1] + Predict(diffs, forward),
            (false, false) => numbers[0] - Predict(diffs, forward)
        };
    }
}
