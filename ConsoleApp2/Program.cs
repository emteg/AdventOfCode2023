using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Console.WriteLine(Part1(new StringReader(@"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green").EnumerateLines(), 12, 13, 14));
        
        Console.WriteLine(Part1(File.ReadLines("Input.txt"), 12, 13, 14));
        
        Console.WriteLine(Part2(new StringReader(@"Game 1: 3 blue, 4 red; 1 red, 2 green, 6 blue; 2 green
Game 2: 1 blue, 2 green; 3 green, 4 blue, 1 red; 1 green, 1 blue
Game 3: 8 green, 6 blue, 20 red; 5 blue, 4 red, 13 green; 5 green, 1 red
Game 4: 1 green, 3 red, 6 blue; 3 green, 6 red; 3 green, 15 blue, 14 red
Game 5: 6 red, 1 blue, 3 green; 2 blue, 1 red, 2 green").EnumerateLines()));
        
        Console.WriteLine(Part2(File.ReadLines("Input.txt")));
    }

    private static uint Part1(IEnumerable<string> lines, uint red, uint green, uint blue)
    {
        return lines
            .Select(ParseGame)
            .Where(game => game.IsPossibleWith(red, green, blue))
            .Select(game => game.Id)
            .Sum();
    }

    private static uint Part2(IEnumerable<string> lines)
    {
        return lines
            .Select(ParseGame).ToList()
            .Select(game => game.MinimumNumberOfCubesRequired)
            .Select(set => set.red * set.green * set.blue)
            .Sum();
    }

    private static Game ParseGame(string line) => IdAndSetsOfCubesAndColors(IdAndSets(line));


    private static (uint Id, string[] Sets) IdAndSets(string line)
    {
        string[] parts = line.Substring(5).Split(": ");
        return (uint.Parse(parts[0]), parts[1].Split("; "));
    }

    private static Game IdAndSetsOfCubesAndColors((uint Id, string[] Sets) game)
    {
        var sets = game.Sets
            .Select(set => set.Split(", "))
            .Select(sets => sets.Select(cubes => cubes.Split(' ')).ToArray())
            .Select(sets => sets.Select(cubes => new Cubes(uint.Parse(cubes[0]), cubes[1])))
            .Select(sets => sets.ToArray());

        return new Game(game.Id, sets);
    }
    
    private struct Game
    {
        public readonly uint Id;
        public readonly IReadOnlyList<Cubes[]> SetsOfCubes;

        public Game(uint id, IEnumerable<Cubes[]> setsOfCubes)
        {
            Id = id;
            SetsOfCubes = new List<Cubes[]>(setsOfCubes);
        }

        public (uint red, uint green, uint blue) MinimumNumberOfCubesRequired
        {
            get
            {
                if (minimumNumberOfCubesRequired is not null)
                    return minimumNumberOfCubesRequired.Value;

                minimumNumberOfCubesRequired = (0, 0, 0);
                foreach (Cubes[] set in SetsOfCubes)
                {
                    foreach (Cubes cubes in set)
                    {
                        if (cubes.Color == "red" && cubes.Number > minimumNumberOfCubesRequired.Value.red)
                            minimumNumberOfCubesRequired = (cubes.Number, minimumNumberOfCubesRequired.Value.green, minimumNumberOfCubesRequired.Value.blue);
                        else if (cubes.Color == "green" && cubes.Number > minimumNumberOfCubesRequired.Value.green)
                            minimumNumberOfCubesRequired = (minimumNumberOfCubesRequired.Value.red, cubes.Number, minimumNumberOfCubesRequired.Value.blue);
                        else if (cubes.Color == "blue" && cubes.Number > minimumNumberOfCubesRequired.Value.blue)
                            minimumNumberOfCubesRequired = (minimumNumberOfCubesRequired.Value.red, minimumNumberOfCubesRequired.Value.green, cubes.Number);
                    }
                }

                return minimumNumberOfCubesRequired.Value;
            }
        }

        public bool IsPossibleWith(uint red, uint green, uint blue)
        {
            foreach (Cubes[] set in SetsOfCubes)
            {
                foreach (Cubes cubes in set)
                {
                    if (cubes.Color == "red" && cubes.Number > red)
                        return false;
                    if (cubes.Color == "green" && cubes.Number > green)
                        return false;
                    if (cubes.Color == "blue" && cubes.Number > blue)
                        return false;
                }
            }
            
            return true;
        }

        private (uint red, uint green, uint blue)? minimumNumberOfCubesRequired = null;
    }

    private readonly struct Cubes
    {
        public readonly uint Number;
        public readonly string Color;

        public Cubes(uint number, string color)
        {
            Number = number;
            Color = color;
        }
    }
}