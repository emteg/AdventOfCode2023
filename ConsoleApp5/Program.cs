// See https://aka.ms/new-console-template for more information

using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var sampleInput = new StringReader(@"seeds: 79 14 55 13

seed-to-soil map:
50 98 2
52 50 48

soil-to-fertilizer map:
0 15 37
37 52 2
39 0 15

fertilizer-to-water map:
49 53 8
0 11 42
42 0 7
57 7 4

water-to-light map:
88 18 7
18 25 70

light-to-temperature map:
45 77 23
81 45 19
68 64 13

temperature-to-humidity map:
0 69 1
1 0 69

humidity-to-location map:
60 56 37
56 93 4");

        TextReader input = sampleInput;//File.OpenText("input.txt");//sampleInput;

        //Part1(input);

        List<SeedRange> seedRanges = ParseSeedRanges(input.ReadLine()!).ToList();
        List<Map> maps = Parse(input).ToList();

        ulong lowestLocation = ulong.MaxValue;
        foreach (SeedRange seedRange in seedRanges)
        {
            foreach (ulong seedNumber in seedRange.ContainedIds())
            {
                ulong result = seedNumber;
                foreach (Map map in maps)
                {
                    result = map.Convert(result);
                }

                if (result < lowestLocation)
                    lowestLocation = result;
            }
        }
        
        Console.WriteLine(lowestLocation);
    }

    private static IEnumerable<SeedRange> ParseSeedRanges(string line)
    {
        State state = State.SearchNumber;
        int numberStart = -1;
        int numberLength = -1;
        uint? firstNumber = null;
        for (int i = 0; i < line.Length; i++)
        {
            bool isNumber = line[i].IsNumber();
            bool firstNumberIsNull = firstNumber is null;

            (state, SeedRange? range) = (state, isNumber, firstNumberIsNull) switch
            {
                (State.SearchNumber, false, _) => (state, null),
                (State.SearchNumber, true, _) => StartNumber(i),
                (State.ReadNumber, true, _) => ContinueNumber(),
                (State.ReadNumber, false, true) => FinishFirstNumber(),
                (State.ReadNumber, false, false) => FinishSecondNumber(),
                _ => throw new InvalidOperationException()
            };
            if (range is not null)
                yield return range;
        }

        if (state is State.ReadNumber && firstNumber is null)
            throw new InvalidOperationException("Finished line without a 2nd seed number");

        if (state is State.ReadNumber && firstNumber is not null)
        {
            (_, SeedRange? range) = FinishSecondNumber();
            yield return range!;
        }

        (State, SeedRange?) StartNumber(int index)
        {
            numberStart = index;
            numberLength = 1;
            return (State.ReadNumber, null);
        }

        (State, SeedRange?) ContinueNumber()
        {
            numberLength++;
            return (state, null);
        }

        (State, SeedRange?) FinishFirstNumber()
        {
            firstNumber = uint.Parse(line.Substring(numberStart, numberLength));
            numberStart = -1;
            numberLength = -1;
            return (State.SearchNumber, null);
        }

        (State, SeedRange?) FinishSecondNumber()
        {
            uint secondNumber = uint.Parse(line.Substring(numberStart, numberLength));
            SeedRange result = new(firstNumber!.Value, secondNumber);
            numberStart = -1;
            numberLength = -1;
            firstNumber = null;
            return (State.SearchNumber, result);
        }
    }

    private static void Part1(TextReader input)
    {
        List<uint> seedNumbers = input
            .ReadLine()!
            .Split(' ')
            .Skip(1)
            .Select(uint.Parse)
            .ToList();

        List<Map> maps = Parse(input).ToList();

        List<ulong> locations = new(seedNumbers.Count);
        foreach (uint seedNumber in seedNumbers)
        {
            ulong result = seedNumber;
            foreach (Map map in maps)
            {
                result = map.Convert(result);
            }

            locations.Add(result);
        }

        Console.WriteLine(locations.Min());
    }

    private static IEnumerable<Map> Parse(TextReader input)
    {
        State state = State.SearchTitle;
        Map? map = null;
        foreach (string line in input.EnumerateLines())
        {
            bool yieldMap = false;
            (state, yieldMap) = (state, line) switch
            {
                (State.SearchTitle,  "") => (state,                         false),
                (State.SearchTitle,  _)      => (ParseTitleLine(line, ref map), false),
                (State.SearchNumber, "") => (State.SearchTitle,             true),
                (State.SearchNumber, _)      => (AddRangeToMap(line, map!),     false),
                _ => throw new InvalidOperationException()
            };
            if (yieldMap)
                yield return map!;
        }
        if (state is State.SearchNumber)
            yield return map!;
    }

    private static State ParseTitleLine(string line, ref Map? map)
    {
        map = new Map(line.Split(':')[0]);
        return State.SearchNumber;
    }

    private static State AddRangeToMap(string line, Map map)
    {
        map.AddRange(ParseRangeLine(line));
        return State.SearchNumber;
    }

    private static Range ParseRangeLine(string line)
    {
        int numberStart = -1;
        int numberLength = -1;
        List<uint> numbers = new(3);
        State state = State.SearchNumber;
        
        for (int i = 0; i < line.Length; i++)
        {
            bool isNumber = line[i].IsNumber();
            state = (state, isNumber) switch
            {
                (State.SearchNumber, false) => state,
                (State.SearchNumber, true) => StartNumber(i),
                (State.ReadNumber, true) => ContinueNumber(),
                (State.ReadNumber, false) => FinishNumber(),
                _ => throw new InvalidOperationException()
            };
        }

        if (state is State.ReadNumber)
            FinishNumber();

        if (numbers.Count != 3)
            throw new InvalidOperationException("Unexpected number of numbers!");

        return new Range(numbers[0], numbers[1], numbers[2]);

        State StartNumber(int index)
        {
            numberStart = index;
            numberLength = 1;
            return State.ReadNumber;
        }

        State ContinueNumber()
        {
            numberLength++;
            return state;
        }

        State FinishNumber()
        {
            numbers.Add(uint.Parse(line.Substring(numberStart, numberLength)));
            numberStart = -1;
            numberLength = -1;
            return State.SearchNumber;
        }
    }

    private record Map(string Title)
    {
        public IReadOnlyList<Range> Ranges => ranges;

        public void AddRange(Range range)
        {
            ranges.Add(range);
        }

        public ulong Convert(ulong sourceId)
        {
            foreach (Range range in ranges)
                if (range.Contains(sourceId))
                    return range.Convert(sourceId);

            return sourceId;
        }

        private readonly List<Range> ranges = new();
    }

    private record Range(ulong DestinationStart, ulong SourceStart, uint Length)
    {
        public bool Contains(ulong sourceId) => sourceId >= SourceStart && sourceId <= sourceEnd;

        public ulong Convert(ulong sourceId)
        {
            if (!Contains(sourceId))
                return sourceId;
            
            ulong delta = sourceId - SourceStart;
            return DestinationStart + delta;
        }
        
        private readonly ulong sourceEnd = SourceStart + Length - 1;
    }

    private record SeedRange(ulong Start, uint Length)
    {
        public IEnumerable<ulong> ContainedIds()
        {
            for (ulong i = Start; i < Start + Length; i++)
                yield return i;
        }
    }

    private enum State
    {
        SearchTitle, // looks for non-empty line
        SearchNumber, // looks for numeric char, or end of line, or empty line
        ReadNumber, // looks for non-numeric char, or end of lilne
    }
}