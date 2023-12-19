// See https://aka.ms/new-console-template for more information

using System.Collections;
using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        StringReader sampleInput1 = new(@"-L|F7
7S-7|
L|7||
-L-J|
L|-JF");

        StringReader sampleInput2 = new(@"..F7.
.FJ|.
SJ.L7
|F--J
LJ...");

        StringReader sampleInput3 = new(@"..........
.S------7.
.|F----7|.
.||....||.
.||....||.
.|L-7F-J|.
.|..||..|.
.L--JL--J.
..........");

        StringReader sampleInput4 = new(@".F----7F7F7F7F-7....
.|F--7||||||||FJ....
.||.FJ||||||||L7....
FJL7L7LJLJ||LJ.L-7..
L--J.L7...LJS7F-7L7.
....F-J..F7FJ|L7L7L7
....L7.F7||L7|.L7L7|
.....|FJLJ|FJ|F7|.LJ
....FJL-7.||.||||...
....L---J.LJ.LJLJ...");


        //Tile[][] input = ReadInput(sampleInput1.EnumerateLines().ToList());
        //Tile[][] input = ReadInput(sampleInput2.EnumerateLines().ToList());
        //Tile[][] input = ReadInput(sampleInput3.EnumerateLines().ToList());
        Tile[][] input = ReadInput(sampleInput4.EnumerateLines().ToList());
        //Tile[][] input = ReadInput(File.OpenText("input.txt").EnumerateLines().ToList());

        (uint x, uint y) start = FindStart(input);
        Console.WriteLine($"Start is at ({start.x}|{start.y})");
        uint length = FindLoop(start, input);
        uint tilesInside = FindTilesInside(input);
        PrintLoop(input);
        Console.WriteLine($"Loop length: {length}");
        Console.WriteLine($"Tiles inside of loop: {tilesInside}");
    }

    private static uint FindTilesInside(Tile[][] input)
    {
        bool isInside = false;
        uint tilesInside = 0;
        for (uint y = 0; y < input.Length; y++)
        {
            for (uint x = 0; x < input[0].Length; x++)
            {
                Tile tile = input[y][x];
                bool tileIsPartOfLoop = tile.IsPartOfLoop;
                bool canGoEast = CanGoEastFrom(x, y, input);
                isInside = (tileIsPartOfLoop, canGoEast, isInside) switch
                {
                    (true, true, false) => true,
                    (true, true, _) => isInside,
                    (true, false, _) => !isInside,
                    (false, _, _) => isInside
                };
                tile.IsInside = isInside;
                if (isInside && !tile.IsPartOfLoop)
                    tilesInside++;
            }
            isInside = false;
        }

        return tilesInside;
    }

    private static void PrintLoop(Tile[][] input)
    {
        Console.ResetColor();
        ConsoleColor defaultColor = Console.ForegroundColor;
        for (int y = 0; y < input.Length; y++)
        {
            for (int x = 0; x < input[0].Length; x++)
            {
                bool isStart = input[y][x].Pipe is Pipe.Start;
                Console.ForegroundColor = (input[y][x].IsPartOfLoop, input[y][x].IsInside, isStart) switch
                {
                    (true, _, false) => ConsoleColor.Red,
                    (true, _, true) => ConsoleColor.Yellow,
                    (_, null, _) => defaultColor,
                    (_, false, _) => defaultColor,
                    (_, true, _) => ConsoleColor.Cyan
                };
                Console.Write(ToChar(input[y][x].Pipe));
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    private static uint FindLoop((uint x, uint y) start, Tile[][] input)
    {
        if (input[start.y][start.x].Pipe is not Pipe.Start)
            throw new InvalidOperationException();

        uint loopLength = 0;
        (uint x, uint y) previous = start;
        (uint x, uint y) current = TilesReachableFrom(start.x, start.y, input).First();

        while (current != start)
        {
            loopLength++;
            input[current.y][current.x].IsPartOfLoop = true;
            var temp = previous;
            previous = current;
            current = TilesReachableFrom(current.x, current.y, input, temp.x, temp.y).First();
        }
        input[current.y][current.x].IsPartOfLoop = true;

        return (uint)Math.Ceiling(loopLength / 2.0);
    }

    private static IEnumerable<(uint x, uint y)> TilesReachableFrom(uint x, uint y, Tile[][] grid, uint? xFrom = null,
        uint? yFrom = null)
    {
        if (CanGoWestFrom(x, y, grid) && xFrom != x - 1)
            yield return (x - 1, y);
        if (CanGoNorthFrom(x, y, grid) && yFrom != y - 1)
            yield return (x, y - 1);
        if (CanGoEastFrom(x, y, grid) && xFrom != x + 1)
            yield return (x + 1, y);
        if (CanGoSouthFrom(x, y, grid) && yFrom != y + 1)
            yield return (x, y + 1);
    }

    private static bool CanGoWestFrom(uint x, uint y, Tile[][] grid)
    {
        if (x == 0)
            return false;

        Pipe start = grid[y][x].Pipe;
        Pipe left = grid[y][x - 1].Pipe;

        bool startCanGoWest = start is Pipe.Start or Pipe.EastWest or Pipe.BendNorthWest or Pipe.BendSouthWest;
        if (!startCanGoWest)
            return false;

        return left is Pipe.Start or Pipe.EastWest or Pipe.BendNorthEast or Pipe.BendSouthEast;
    }
    
    private static bool CanGoEastFrom(uint x, uint y, Tile[][] grid)
    {
        if (x == grid[0].Length - 1)
            return false;

        Pipe start = grid[y][x].Pipe;
        Pipe right = grid[y][x + 1].Pipe;

        bool startCanGoEast = start is Pipe.Start or Pipe.EastWest or Pipe.BendNorthEast or Pipe.BendSouthEast;
        if (!startCanGoEast)
            return false;

        return right is Pipe.Start or Pipe.EastWest or Pipe.BendNorthWest or Pipe.BendSouthWest;
    }
    
    private static bool CanGoNorthFrom(uint x, uint y, Tile[][] grid)
    {
        if (y == 0)
            return false;

        Pipe start = grid[y][x].Pipe;
        Pipe top = grid[y - 1][x].Pipe;

        bool startCanGoNorth = start is Pipe.Start or Pipe.NorthSouth or Pipe.BendNorthWest or Pipe.BendNorthEast;
        if (!startCanGoNorth)
            return false;

        return top is Pipe.Start or Pipe.NorthSouth or Pipe.BendSouthEast or Pipe.BendSouthWest;
    }
    
    private static bool CanGoSouthFrom(uint x, uint y, Tile[][] grid)
    {
        if (y == grid.Length - 1)
            return false;

        Pipe start = grid[y][x].Pipe;
        Pipe bottom = grid[y + 1][x].Pipe;

        bool startCanGoSouth = start is Pipe.Start or Pipe.NorthSouth or Pipe.BendSouthEast or Pipe.BendSouthWest;
        if (!startCanGoSouth)
            return false;

        return bottom is Pipe.Start or Pipe.NorthSouth or Pipe.BendNorthWest or Pipe.BendNorthEast;
    }

    private static (uint x, uint y) FindStart(Tile[][] input)
    {
        for (uint y = 0; y < input.Length; y++)
        {   
            Tile[] line = input[y];
            for (uint x = 0; x < line.Length; x++)
            {
                Tile tile = line[x];
                if (tile.Pipe is Pipe.Start)
                {
                    tile.IsPartOfLoop = true;
                    return (x, y);
                }
            }
        }

        throw new ArgumentException("Start not found :/");
    }

    private static Tile[][] ReadInput(IReadOnlyList<string> lines)
    {
        Tile[][] result = new Tile[lines.Count][];
        for (int y = 0; y < lines.Count; y++)
        {
            result[y] = lines[y].Select(ToPipe).Select(p => new Tile(p)).ToArray();
        }
        return result;
    }
    
    private static Pipe ToPipe(char c)
    {
        return c switch
        {
            '.' => Pipe.Ground,
            'S' => Pipe.Start,
            '-' => Pipe.EastWest,
            '|' => Pipe.NorthSouth,
            'L' => Pipe.BendNorthEast,
            'J' => Pipe.BendNorthWest,
            '7' => Pipe.BendSouthWest,
            'F' => Pipe.BendSouthEast,
            _ => throw new ArgumentOutOfRangeException(nameof(c))
        };
    }
    
    private static char ToChar(Pipe p)
    {
        return p switch
        {
             Pipe.Ground => '\u2591',
             Pipe.Start => 'S',
             Pipe.EastWest => '\u2550',
             Pipe.NorthSouth => '\u2551',
             Pipe.BendNorthEast => '\u255a',
             Pipe.BendNorthWest => '\u255d',
             Pipe.BendSouthWest => '\u2557',
             Pipe.BendSouthEast => '\u2554',
            _ => throw new ArgumentOutOfRangeException(nameof(p))
        };
    }

    private record Tile(Pipe Pipe)
    {
        public bool IsPartOfLoop { get; set; } = false;
        public bool? IsInside { get; set; } = null;
    }

    private enum Pipe
    {
        Ground,
        Start,
        EastWest,
        NorthSouth,
        BendNorthEast,
        BendNorthWest,
        BendSouthWest,
        BendSouthEast,
    }
}