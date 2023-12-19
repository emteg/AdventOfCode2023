// See https://aka.ms/new-console-template for more information

using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        StringReader sampleInput = new(@"...#......
.......#..
#.........
..........
......#...
.#........
.........#
..........
.......#..
#...#.....");

        //Universe universe = new(sampleInput.EnumerateLines());
        Universe universe = new(File.OpenText("input.txt").EnumerateLines());

        Part1(universe);
        Part2(universe);
    }

    private static void Part1(Universe universe)
    {
        long pathLengths = universe
            .GalaxyPairsInGiantUniverse(2)
            .Select(pair => ManhattanDistance(pair.x1, pair.y1, pair.x2, pair.y2))
            .Sum();
        Console.WriteLine(pathLengths);
    }

    private static void Part2(Universe universe)
    {
        long pathLengths = universe
            .GalaxyPairsInGiantUniverse()
            .Select(pair => ManhattanDistance(pair.x1, pair.y1, pair.x2, pair.y2))
            .Aggregate(0L, (total, current) => total + current);
        Console.WriteLine(pathLengths);
    }

    private static int ManhattanDistance(int x1, int y1, int x2, int y2) => Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
}

public sealed class Universe
{
    public Universe(IEnumerable<string> image)
    {
        foreach (string line in image)
        {
            lines.Add(new List<char>(line.ToCharArray()));
        }
    }

    public IEnumerable<(int x, int y)> Galaxies()
    {
        for (int y = 0; y < lines.Count; y++)
        {
            List<char> line = lines[y];
            for (int x = 0; x < line.Count; x++)
            {
                if (line[x] == '#')
                    yield return (x, y);
            }
        }
    }

    public IEnumerable<(int x1, int y1, int x2, int y2)> GalaxyPairsInGiantUniverse(int emptySpaceFactor = 1_000_000)
    {
        List<int> emptyRowIndices = new();
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].All(IsEmptySpace)) 
                emptyRowIndices.Add(i);
        }
        
        List<int> emptyColIndices = new();
        for (int i = lines[0].Count - 1; i >= 0; i--)
        {
            if (Column(i).All(IsEmptySpace)) 
                emptyColIndices.Add(i);
        }
        
        List<(int x, int y)> galaxies = Galaxies().ToList();
        for (int i = 0; i < galaxies.Count; i++)
        {
            (int x, int y) galaxy1 = galaxies[i];
            for (int j = i + 1; j < galaxies.Count; j++)
            {
                (int x, int y) galaxy2 = galaxies[j];

                int emptyColsBeforeGalaxy1 = emptyColIndices.Count(it => it < galaxy1.x);
                int emptyColsBeforeGalaxy2 = emptyColIndices.Count(it => it < galaxy2.x);

                int emptyRowsBeforeGalaxy1 = emptyRowIndices.Count(it => it < galaxy1.y);
                int emptyRowsBeforeGalaxy2 = emptyRowIndices.Count(it => it < galaxy2.y);

                int x1 = galaxy1.x + emptyColsBeforeGalaxy1 * (emptySpaceFactor - 1);
                int x2 = galaxy2.x + emptyColsBeforeGalaxy2 * (emptySpaceFactor - 1);

                int y1 = galaxy1.y + emptyRowsBeforeGalaxy1 * (emptySpaceFactor - 1);
                int y2 = galaxy2.y + emptyRowsBeforeGalaxy2 * (emptySpaceFactor - 1);
                
                yield return (x1, y1, x2, y2);
            }
        }
    }

    private static bool IsEmptySpace(char c) => c == '.';

    private IEnumerable<char> Column(int columnIndex) => lines.Select(list => list[columnIndex]);

    private readonly List<List<char>> lines = new();
}