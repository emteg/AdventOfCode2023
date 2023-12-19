// See https://aka.ms/new-console-template for more information

using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader sample = new StringReader(@"O....#....
O.OO#....#
.....##...
OO.#O....O
.O.....O#.
O.#..O.#.#
..O..#O..O
.......O..
#....###..
#OO..#....");
        Part1(sample);
        Part1(File.OpenText("input.txt"));

        Part2(sample, 9, 2);
        Part2(File.OpenText("input.txt"), 140, 102);
    }

    private static void Part2(TextReader input, int cyclesToRun, int repetitionStartIndex)
    {
        char[][] grid = ReadGrid(input);
        List<LoadCycle> loadCycles = CreateCycleGraph(grid, cyclesToRun, repetitionStartIndex);

        LoadCycle cycle = loadCycles[0];
        for (int i = 1; i < 1000000000; i++) 
            cycle = cycle.Next!;
        Console.WriteLine(cycle.Value);
    }

    private static List<LoadCycle> CreateCycleGraph(char[][] grid, int cyclesToRun, int repetitionStartIndex)
    {
        List<LoadCycle> loadCycles = new();
        
        LoadCycle? previous = null;
        for (int i = 0; i < cyclesToRun; i++)
        {
            TiltNorth(grid);
            TiltWest(grid);
            TiltSouth(grid);
            TiltEast(grid);
            LoadCycle newCycle = new(CalculateLoadOnNorthBorder(grid));
            if (previous is not null)
                previous.Next = newCycle;
            loadCycles.Add(newCycle);
            previous = newCycle;
        }
        loadCycles.Last().Next = loadCycles[repetitionStartIndex];

        return loadCycles;
    }

    private sealed class LoadCycle
    {
        public uint Value { get; init; }
        public LoadCycle? Next = null;

        public LoadCycle(uint value)
        {
            this.Value = value;
        }
    }

    private static void Part1(TextReader sample)
    {
        char[][] grid = ReadGrid(sample);
        TiltNorth(grid);
        Print(grid);
        uint load = CalculateLoadOnNorthBorder(grid);
        Console.WriteLine(load);
    }

    private static uint CalculateLoadOnNorthBorder(char[][] grid)
    {
        uint rowFactor = (uint)grid.Length;
        uint result = 0;

        for (int row = 0; row < grid.Length; row++)
        {
            uint rocks = (uint)grid[row].Count(c => c == 'O');
            result += rocks * rowFactor;
            rowFactor--;
        }
        
        return result;
    }

    private static void Print(char[][] grid)
    {
        foreach (char[] line in grid)
            Console.WriteLine(new string(line));
    }

    private static void TiltNorth(char[][] grid)
    {
        for (int x = 0; x < grid[0].Length; x++)
        {
            for (int y = 1; y < grid.Length; y++)
            {
                if (grid[y][x] != 'O')
                    continue;

                for (int i = y - 1; i >= 0; i--)
                {
                    if (grid[i][x] != '.')
                        break;

                    grid[i][x] = grid[i + 1][x];
                    grid[i + 1][x] = '.';
                }
            }
        }
    }
    
    private static void TiltEast(char[][] grid)
    {
        foreach (char[] line in grid)
        {
            for (int x = line.Length - 2; x >= 0; x--)
            {
                if (line[x] != 'O')
                    continue;

                for (int i = x; i < line.Length - 1; i++)
                {
                    if (line[i + 1] != '.')
                        break;

                    line[i + 1] = line[i];
                    line[i] = '.';
                }
            }
        }
    }
    
    private static void TiltSouth(char[][] grid)
    {
        for (int x = 0; x < grid[0].Length; x++)
        {
            for (int y = grid.Length - 2; y >= 0 ; y--)
            {
                if (grid[y][x] != 'O')
                    continue;

                for (int i = y; i < grid.Length - 1; i++)
                {
                    if (grid[i + 1][x] != '.')
                        break;

                    grid[i + 1][x] = grid[i][x];
                    grid[i][x] = '.';
                }
            }
        }
    }
    
    private static void TiltWest(char[][] grid)
    {
        foreach (char[] line in grid)
        {
            for (int x = 1; x < line.Length; x++)
            {
                if (line[x] != 'O')
                    continue;

                for (int i = x; i > 0; i--)
                {
                    if (line[i - 1] != '.')
                        break;

                    line[i - 1] = line[i];
                    line[i] = '.';
                }
            }
        }
    }

    private static char[][] ReadGrid(TextReader reader)
    {
        List<string> lines = reader.EnumerateLines().ToList();
        char[][] grid = new char[lines.Count][];
        for (int i = 0; i < lines.Count; i++)
        {
            grid[i] = lines[i].ToCharArray();
        }

        return grid;
    }
}