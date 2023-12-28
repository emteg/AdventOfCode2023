// See https://aka.ms/new-console-template for more information

using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sampleInput1 = @"-L|F7
7S-7|
L|7||
-L-J|
L|-JF";

        string sampleInput2 = @"..F7.
.FJ|.
SJ.L7
|F--J
LJ...";

        string sampleInput3 = @"..........
.S------7.
.|F----7|.
.||....||.
.||....||.
.|L-7F-J|.
.|..||..|.
.L--JL--J.
..........";

        string sampleInput4 = @".F----7F7F7F7F-7....
.|F--7||||||||FJ....
.||.FJ||||||||L7....
FJL7L7LJLJ||LJ.L-7..
L--J.L7...LJS7F-7L7.
....F-J..F7FJ|L7L7L7
....L7.F7||L7|.L7L7|
.....|FJLJ|FJ|F7|.LJ
....FJL-7.||.||||...
....L---J.LJ.LJLJ...";

        string sampleInput5 = @"FF7FSF7F7F7F7F7F---7
L|LJ||||||||||||F--J
FL-7LJLJ||||||LJL-77
F--JF--7||LJLJ7F7FJ-
L---JF-JLJ.||-FJLJJ7
|F|F-JF---7F7-L7L|7|
|FFJF7L7F-JF7|JL---7
7-L-JL7||F7|L7F-7F7|
L.L7LFJ|||||FJL7||LJ
L7JLJL-JLJLJL--JLJ.L";

        Console.WriteLine(Part1(new StringReader(sampleInput1).EnumerateLines()));
        Console.WriteLine(Part1(new StringReader(sampleInput2).EnumerateLines()));
        Console.WriteLine(Part1(new StringReader(sampleInput3).EnumerateLines()));
        Console.WriteLine(Part1(new StringReader(sampleInput4).EnumerateLines()));
        Console.WriteLine(Part1(File.OpenText("input.txt").EnumerateLines()));
        
        Console.WriteLine(Part2(new StringReader(sampleInput1).EnumerateLines()));
        Console.WriteLine(Part2(new StringReader(sampleInput2).EnumerateLines()));
        Console.WriteLine(Part2(new StringReader(sampleInput3).EnumerateLines()));
        Console.WriteLine(Part2(new StringReader(sampleInput4).EnumerateLines()));
        Console.WriteLine(Part2(new StringReader(sampleInput5).EnumerateLines()));
        Console.WriteLine(Part2(File.OpenText("input.txt").EnumerateLines()));
    }
    
    private static uint Part1(IEnumerable<string> input)
    {
        Tile[][] grid = ReadInput(input.ToList());
        (uint x, uint y) start = FindStart(grid);
        Console.WriteLine($"Start is at ({start.x}|{start.y})");
        
        uint loopLength = FindLoop(start, grid);
        
        PrintLoop(grid);
        return loopLength;
    }

    private static uint Part2(IEnumerable<string> input)
    {
        Tile[][] grid = ReadInput(input.ToList());
        (uint x, uint y) start = FindStart(grid);
        FindLoop(start, grid);
        Tile[][] superSampled = CreateSuperSampledGrid(grid);
        FindTilesEnclosedBySuperSampledLoop(superSampled);

        uint tilesEnclosed = UpdateOriginalGridInsideStatus(grid);
        
        PrintLoop(grid);
        return tilesEnclosed;
    }

    private static uint UpdateOriginalGridInsideStatus(Tile[][] grid)
    {
        uint result = 0;
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[0].Length; x++)
            {
                Tile tile = grid[y][x];

                if (tile.Pipe is Pipe.Ground)
                {
                    tile.IsInside = tile.SuperSampled[0][0].IsInside;
                    if (tile.IsInside is true)
                        result++;
                    continue;
                }

                if (tile.IsPartOfLoop)
                {
                    tile.IsInside = false;
                    continue;
                }
                
                tile.IsInside = tile.SuperSampled[0][0].IsInside;
                if (tile.IsInside is true)
                    result++;
            }
        }

        return result;
    }

    private static Tile[][] CreateSuperSampledGrid(Tile[][] grid)
    {
        for (int y = 0; y < grid.Length; y++)
            for (int x = 0; x < grid[0].Length; x++) 
                grid[y][x].SuperSample(grid, x, y);

        Tile[][] superSampled = new Tile[grid.Length * 3][];
        for (int y = 0; y < superSampled.Length; y++) 
            superSampled[y] = new Tile[grid[0].Length * 3];
        
        for (int gridY = 0; gridY < grid.Length; gridY++)
        {
            for (int gridX = 0; gridX < grid[0].Length; gridX++)
            {
                for (int subY = 0; subY < 3; subY++) 
                {
                    for (int subX = 0; subX < 3; subX++)
                    {
                        int y = 3 * gridY + subY;
                        int x = 3 * gridX + subX;
                        superSampled[y][x] = grid[gridY][gridX].SuperSampled[subY][subX];
                    }
                }
            }
        }

        return superSampled;
    }

    private static void FindTilesEnclosedBySuperSampledLoop(Tile[][] input)
    {
        int dx = -1;
        int dy = -1;
        int tilesInfected = 0;
        
        do
        {
            tilesInfected = 0;

            if (dx < 0)
            {
                for (int y = 0; y < input.Length; y++)
                {
                    for (int x = 0; x < input[0].Length; x++)
                    {
                        tilesInfected += InfectTile(input, y, x, dy, dx);
                    }
                }
            }
            else
            {
                for (int y = input.Length - 1; y >= 0; y--)
                {
                    for (int x = input[0].Length - 1; x >= 0; x--)
                    {
                        tilesInfected += InfectTile(input, y, x, dy, dx);
                    }
                }
            }

            if (dx < 0)
            {
                dx = 1;
                dy = 1;
            }
            else
            {
                dx = -1;
                dy = -1;
            }
            
        } while (tilesInfected > 0);
        
        foreach (Tile[] line in input)
        {
            for (int x = 0; x < input[0].Length; x++)
            {
                if (line[x].IsInside is not null)
                    continue;
                
                line[x].IsInside = true;
            }
        }
    }

    private static byte InfectTile(Tile[][] input, int y, int x, int dy, int dx)
    {
        Tile tile = input[y][x];

        if (tile.IsInside is not null)
            return 0;

        if (y == 0 || y == input.Length - 1 || x == 0 || x == input[0].Length - 1)
        {
            tile.IsInside = false;
            return 1;
        }

        if (tile.IsPartOfLoop)
        {
            tile.IsInside = false;
            return 1;
        }

        Tile previousRow = input[y + dy][x];
        Tile previousCol = input[y][x + dx];

        if (!previousRow.IsPartOfLoop && previousRow.IsInside is not null)
        {
            tile.IsInside = previousRow.IsInside;
            return 1;
        }

        if (!previousCol.IsPartOfLoop && previousCol.IsInside is not null)
        {
            tile.IsInside = previousCol.IsInside;
            return 1;
        }

        return 0;
    }

    private static void PrintLoop(Tile[][] grid)
    {
        Console.ResetColor();
        ConsoleColor defaultColor = Console.ForegroundColor;
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[0].Length; x++)
            {
                bool isStart = grid[y][x].Pipe is Pipe.Start;
                Console.ForegroundColor = (grid[y][x].IsPartOfLoop, grid[y][x].IsInside, isStart) switch
                {
                    (true, _, false) => ConsoleColor.Red,
                    (_, _, true) => ConsoleColor.Yellow,
                    (_, true, _) => ConsoleColor.Cyan,
                    _ => defaultColor,
                };
                Console.Write(ToChar(grid[y][x].Pipe));
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }

    private static uint FindLoop((uint x, uint y) start, Tile[][] grid)
    {
        if (grid[start.y][start.x].Pipe is not Pipe.Start)
            throw new InvalidOperationException();

        uint loopLength = 0;
        (uint x, uint y) previous = start;
        (uint x, uint y) current = TilesReachableFrom(start.x, start.y, grid).First();

        while (current != start)
        {
            loopLength++;
            grid[current.y][current.x].IsPartOfLoop = true;
            var temp = previous;
            previous = current;
            current = TilesReachableFrom(current.x, current.y, grid, temp.x, temp.y).First();
        }
        grid[current.y][current.x].IsPartOfLoop = true;

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

    private static (uint x, uint y) FindStart(Tile[][] grid)
    {
        for (uint y = 0; y < grid.Length; y++)
        {   
            Tile[] line = grid[y];
            for (uint x = 0; x < line.Length; x++)
            {
                Tile tile = line[x];
                if (tile.Pipe is Pipe.Start)
                {
                    tile.IsPartOfLoop = true;
                    tile.IsStart = true;
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
        public bool IsStart { get; set; }
        public Tile? Parent { get; init; } = null;

        public Tile[][] SuperSampled { get; private set; } = Array.Empty<Tile[]>();

        public void SuperSample(Tile[][] grid, int tileX, int tileY)
        {
            SuperSampled = new Tile[3][];
            for (int y = 0; y < SuperSampled.Length; y++)
            {
                SuperSampled[y] = new Tile[3];
                for (int x = 0; x < 3; x++)
                {
                    SuperSampled[y][x] = new Tile(Pipe.Ground) {Parent = this};
                }
            }

            if (Pipe is Pipe.Ground)
                return;

            if (Pipe is Pipe.Start)
            {
                if (CanGoNorthFrom((uint)tileX, (uint)tileY, grid) && grid[tileY-1][tileX].IsPartOfLoop)
                    SuperSampled[0][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                if (CanGoSouthFrom((uint)tileX, (uint)tileY, grid) && grid[tileY+1][tileX].IsPartOfLoop)
                    SuperSampled[2][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                if (CanGoWestFrom((uint)tileX, (uint)tileY, grid) && grid[tileY][tileX-1].IsPartOfLoop)
                    SuperSampled[1][0] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                if (CanGoEastFrom((uint)tileX, (uint)tileY, grid) && grid[tileY][tileX+1].IsPartOfLoop)
                    SuperSampled[1][2] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][1] = new Tile(Pipe.Start) { IsPartOfLoop = IsPartOfLoop, IsStart = true, Parent = this };
                return;
            }

            if (Pipe is Pipe.EastWest)
            {
                SuperSampled[1][0] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][1] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][2] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                return;
            }
            
            if (Pipe is Pipe.NorthSouth)
            {
                SuperSampled[0][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[2][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                return;
            }

            if (Pipe is Pipe.BendNorthEast)
            {
                SuperSampled[0][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][1] = new Tile(Pipe.BendNorthEast) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][2] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                return;
            }
            
            if (Pipe is Pipe.BendSouthEast)
            {
                SuperSampled[1][1] = new Tile(Pipe.BendSouthEast) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][2] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[2][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                return;
            }
            
            if (Pipe is Pipe.BendNorthWest)
            {
                SuperSampled[0][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][0] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][1] = new Tile(Pipe.BendNorthWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                return;
            }
            
            if (Pipe is Pipe.BendSouthWest)
            {
                SuperSampled[1][1] = new Tile(Pipe.BendSouthWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[1][0] = new Tile(Pipe.EastWest) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                SuperSampled[2][1] = new Tile(Pipe.NorthSouth) { IsPartOfLoop = IsPartOfLoop, Parent = this };
                return;
            }
        }
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