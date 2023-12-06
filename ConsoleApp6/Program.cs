// See https://aka.ms/new-console-template for more information

using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sample = @"Time:      7  15   30
Distance:  9  40  200";

        string actual = @"Time:        40     82     84     92
Distance:   233   1011   1110   1487";

        string input = actual;
        
        Part1(new StringReader(input));
        Part2(new StringReader(input));
    }

    private static void Part2(TextReader input)
    {
        List<string> lines = input.EnumerateLines().ToList();
        ulong duration = lines[0].Split(':').Skip(1).Select(it => it.Replace(" ", "")).Select(ulong.Parse).First();
        ulong distance = lines[1].Split(':').Skip(1).Select(it => it.Replace(" ", "")).Select(ulong.Parse).First();
        Race race = new(duration, distance);

        Console.WriteLine(race.WaysToWin());
    }

    private static void Part1(TextReader input)
    {
        List<string> lines = input.EnumerateLines().ToList();
        List<ulong> durations = lines[0].Split(' ').Skip(1).Where(it => it.Length > 0).Select(ulong.Parse).ToList();
        List<ulong> distances = lines[1].Split(' ').Skip(1).Where(it => it.Length > 0).Select(ulong.Parse).ToList();
        List<Race> races = new();
        for (int i = 0; i < durations.Count; i++)
        {
            ulong duration = durations[i];
            ulong distance = distances[i];
            races.Add(new Race(duration, distance));
        }

        Console.WriteLine(races.Aggregate(1u, (product, race) => product * race.WaysToWin()));
    }

    private record Race(ulong Duration, ulong BestDistance)
    {
        public uint Acceleration = 1;
        
        public uint WaysToWin()
        {
            uint result = 0;
            
            for (ulong chargeDuration = 0; chargeDuration <= Duration; chargeDuration++)
            {
                ulong speed = chargeDuration * Acceleration;
                ulong distanceTravelled = (Duration - chargeDuration) * speed;
                if (distanceTravelled < BestDistance)
                    continue;
                result++;
            }

            return result;
        }
    }
}