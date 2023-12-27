// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader sample = new StringReader(@"...........
.....###.#.
.###.##..#.
..#.#...#..
....#.#....
.##..S####.
.##..#...#.
.......##..
.##.#.####.
.##..##.##.
...........");

        Tile[][] grid;
        int count;
        
        //(grid, count) = Part1(sample.EnumerateLines(), 6);
        //WriteGrid(grid);
        //Console.WriteLine(count);
        
        (grid, count) = Part1(File.OpenText("input.txt").EnumerateLines(), 64);
        WriteGrid(grid);
        Console.WriteLine(count);
    }

    private static (Tile[][] grid, int count) Part1(IEnumerable<string> lines, int stepsToTake)
    {
        Tile[][] grid = ReadGrid(lines);

        List<Tile> visitedTiles = new();
        HashSet<Tile> nextVisitedTiles = new();
        visitedTiles.AddRange(grid
            .SelectMany(line => line)
            .Where(tile => tile.IsStart));
        
        for (int i = 0; i < stepsToTake; i++)
        {
            foreach (Tile visitedTile in visitedTiles) 
                visitedTile.IsVisited = false;

            nextVisitedTiles.Clear();
            foreach (Tile visitedTile in visitedTiles)
            {
                foreach (Tile tileToVisit in visitedTile.Neighbors().Where(tile => tile.CanBeVisited))
                {
                    tileToVisit.IsVisited = true;
                    nextVisitedTiles.Add(tileToVisit);
                }
            }
            visitedTiles.Clear();
            visitedTiles.AddRange(nextVisitedTiles);
        }

        return (grid, grid.SelectMany(line => line).Count(tile => tile.IsVisited));
    }

    private static void WriteGrid(Tile[][] grid)
    {
        foreach (Tile[] line in grid)
        {
            foreach (Tile tile in line) 
                Console.Write(tile.ToString());
            Console.WriteLine();
        }
    }

    private static Tile[][] ReadGrid(IEnumerable<string> lines)
    {
        List<Tile[]> result = new();

        List<Tile>? previousLine = null;
        foreach (string line in lines)
        {
            Tile? previousTile = null;
            List<Tile> currentLine = new();
            for (int i = 0; i < line.Length; i++)
            {
                Tile tile = Tile.FromChar(line[i]);
                currentLine.Add(tile);
                if (previousTile is not null)
                    tile.ConnectToLeftNeighbor(previousTile);
                if (previousLine is not null)
                    tile.ConnectToTopNeighbor(previousLine[i]);
                previousTile = tile;
            }

            result.Add(currentLine.ToArray());
            previousLine = currentLine;
        }

        return result.ToArray();
    }
}

internal sealed class Tile
{
    public readonly bool CanBeVisited;
    public readonly bool IsStart;
    public bool IsVisited;
    
    public Tile? Top;
    public Tile? Right;
    public Tile? Bottom;
    public Tile? Left;

    public IEnumerable<Tile> Neighbors()
    {
        if (Top is not null)
            yield return Top;
        
        if (Right is not null)
            yield return Right;
        
        if (Bottom is not null)
            yield return Bottom;
        
        if (Left is not null)
            yield return Left;
    }

    public void ConnectToLeftNeighbor(Tile leftNeighbor)
    {
        Left = leftNeighbor;
        leftNeighbor.Right = this;
    }
    
    public void ConnectToTopNeighbor(Tile topNeighbor)
    {
        Top = topNeighbor;
        topNeighbor.Bottom = this;
    }

    public override string ToString()
    {
        if (IsStart)
            return "S";
        if (IsVisited)
            return "O";
        if (CanBeVisited)
            return ".";

        return "#";
    }

    public static Tile FromChar(char c)
    {
        return c switch
        {
            '.' => new Tile(true, false), // garden
            '#' => new Tile(false, false), // rock
            'S' => new Tile(true, true), // start
            _ => throw new InvalidOperationException()
        };
    }
    
    private Tile(bool canBeVisited, bool isStart)
    {
        CanBeVisited = canBeVisited;
        IsStart = isStart;
    }
}