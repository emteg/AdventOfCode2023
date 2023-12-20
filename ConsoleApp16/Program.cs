// See https://aka.ms/new-console-template for more information

using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader sample = new StringReader(@".|...\....
|.-.\.....
.....|-...
........|.
..........
.........\
..../.\\..
.-.-/..|..
.|....-|.\
..//.|....");
        Console.WriteLine(Part1(sample.EnumerateLines()));
        
    }

    private static uint Part1(IEnumerable<string> lines)
    {
        Tile[][] grid = lines
            .Select(it => it.ToCharArray())
            .Select(it => it.Select(c => new Tile(c)).ToArray())
            .ToArray();

        Stack<Beam> beams = new Stack<Beam>();
        beams.Push(new Beam(-1, 0, Direction.Right));

        while (beams.TryPop(out Beam? beam))
        {
            bool stillTravelling = true;
            while (stillTravelling)
            {
                (stillTravelling, Beam? newBeam) = beam.Move(grid);
                if (newBeam is not null)
                    beams.Push(newBeam);
            }
        }
        
        DrawBeams(grid);
        DrawEnergized(grid);
        
        return (uint)grid.SelectMany(line => line).Count(tile => tile.IsEnergized);
    }

    private static void DrawEnergized(Tile[][] grid)
    {
        foreach (Tile[] line in grid)
        {
            foreach (Tile tile in line) 
                Console.Write(tile.IsEnergized ? '#' : '.');
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    
    private static void DrawBeams(Tile[][] grid)
    {
        ConsoleColor defaultColor = Console.ForegroundColor;
        foreach (Tile[] line in grid)
        {
            foreach (Tile tile in line)
            {
                if (tile.IsEnergized)
                    Console.ForegroundColor = ConsoleColor.Red;
                else
                    Console.ForegroundColor = defaultColor;
                if (tile.IsMirror || tile.IsSplitter)
                    Console.Write(tile.Char);
                else if (!tile.IsEnergized)
                    Console.Write(tile.Char);
                else if (tile.Directions.Count == 1)
                    Console.Write(DirectionChar(tile.Directions.First()));
                else
                    Console.Write(tile.Directions.Count);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
        Console.ResetColor();
        Console.WriteLine();
    }

    private static char DirectionChar(Direction direction)
    {
        return direction switch
        {
            Direction.Up => '^',
            Direction.Right => '>',
            Direction.Down => 'v',
            Direction.Left => '<',
            _ => throw new InvalidOperationException()
        };
    }

    private record Beam(int X, int Y, Direction Direction)
    {
        public int X { get; private set; } = X;
        public int Y { get; private set; } = Y;
        public Direction Direction { get; private set; } = Direction;
        
        public (bool stillTravelling, Beam? newBeam) Move(Tile[][] grid)
        {
            MoveToNextTile();
            if (X < 0 || X >= grid[0].Length || Y < 0 || Y >= grid.Length)
                return (false, null);

            Tile newTile = grid[Y][X];

            if (newTile.IsEmpty)
                return (stillTravelling: newTile.Cross(Direction), null);

            if (newTile.IsMirror)
            {
                Direction = Reflect(newTile);
                newTile.Cross(Direction);
                return (true, null);
            }

            if (newTile.IsSplitter)
            {
                (Direction, Beam? newBeam) = Split(newTile);
                newTile.Cross(Direction);
                if (newBeam is not null) 
                    newTile.Cross(newBeam.Direction);
                return (true, newBeam);
            }

            throw new InvalidOperationException();
        }

        private Direction Reflect(Tile tile)
        {
            return (Direction, tile.Char) switch
            {
                (Direction.Right, '/' ) => Direction.Up,
                (Direction.Right, '\\') => Direction.Down,
                (Direction.Left,  '/' ) => Direction.Down,
                (Direction.Left,  '\\') => Direction.Up,
                (Direction.Up,    '/' ) => Direction.Right,
                (Direction.Up,    '\\') => Direction.Left,
                (Direction.Down,  '/' ) => Direction.Left,
                (Direction.Down,  '\\') => Direction.Right,
                _ => throw new InvalidOperationException()
            };
        }

        private (Direction newDirection, Beam? newBeam) Split(Tile tile)
        {
            return (Direction, tile.Char) switch
            {
                (Direction.Left or Direction.Right, '-') => (Direction, null),
                (Direction.Up or Direction.Down, '|') => (Direction, null),
                (Direction.Left or Direction.Right, '|') => (Direction.Up, new Beam(X, Y, Direction.Down)),
                (Direction.Up or Direction.Down, '-') => (Direction.Left, new Beam(X, Y, Direction.Right)),
                _ => throw new InvalidOperationException()
            };
        }

        private void MoveToNextTile()
        {
            int deltaX = Direction is Direction.Right
                ? 1
                : Direction is Direction.Left
                    ? -1
                    : 0;
            int deltaY = Direction is Direction.Down
                ? 1
                : Direction is Direction.Up
                    ? -1
                    : 0;
            X += deltaX;
            Y += deltaY;
        }
    }

    private record Tile(char Char)
    {
        public bool IsEmpty => Char == '.';
        public bool IsMirror => Char is '\\' or '/';
        public bool IsSplitter => Char is '-' or '|';
        public bool IsEnergized { get; private set; }
        public IReadOnlyList<Direction> Directions => directions;

        public void Energize() => IsEnergized = true;
        
        /// <summary>
        /// Marks this tile as energized by a laser and adds the beams
        /// <paramref name="direction"/> to the list of <see cref="Directions"/>,
        /// if that direction is not already there. Returns whether this is the
        /// first time that a beam has crossed this tile with that
        /// <paramref name="direction"/>.
        /// </summary>
        public bool Cross(Direction direction)
        {
            IsEnergized = true;
            if (!directions.Contains(direction))
            {
                directions.Add(direction);
                return true;
            }

            return false;
        }

        private readonly List<Direction> directions = new();
    }
}

