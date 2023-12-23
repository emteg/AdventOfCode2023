// See https://aka.ms/new-console-template for more information

using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        TextReader sample = new StringReader(@"R 6 (#70c710)
D 5 (#0dc571)
L 2 (#5713f0)
D 2 (#d2c081)
R 2 (#59c680)
D 2 (#411b91)
L 5 (#8ceee2)
U 2 (#caa173)
L 1 (#1b58a2)
U 2 (#caa171)
R 2 (#7807d2)
U 3 (#a77fa3)
L 2 (#015232)
U 2 (#7a21e3)");

        Part1(sample.EnumerateLines());
        //Part1(File.OpenText("input.txt").EnumerateLines());
    }

    private static void Part1(IEnumerable<string> input)
    {
        List<DigInstruction> instructions = ReadInstructions(input).ToList();
        DigSite site = new(instructions);
    }
    
    private static IEnumerable<DigInstruction> ReadInstructions(IEnumerable<string> input)
    {
        foreach (string line in input)
        {
            string[] tokens = line.Split(' ');
            Direction dir = tokens[0] switch
            {
                "R" => Direction.Right,
                "D" => Direction.Down,
                "L" => Direction.Left,
                "U" => Direction.Up,
                _ => throw new InvalidOperationException()
            };
            ushort distance = ushort.Parse(tokens[1]);
            string color = tokens[2].Substring(2, 6);
            yield return new DigInstruction(dir, distance, color);
        }
    }
}

internal sealed class DigSite
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public List<Line> Lines { get; } = new();

    public DigSite(IReadOnlyList<DigInstruction> instructions)
    {
        int maxX = 0;
        int minX = 0;
        int maxY = 0;
        int minY = 0;
        int x = 0; // we start with one hole at 0|0
        int y = 0;
        
        foreach (DigInstruction instruction in instructions)
        {
            int startX = x;
            int startY = y;
            (int dx, int dy) = DeltaFromInstruction(instruction);
            x += dx;
            y += dy;
            int endX = x;
            int endY = y;
            Line line = new Line(startX, startY, endX, endY);
            Lines.Add(line);
            if (x > maxX)
                maxX = x;
            else if (x < minX)
                minX = x;
            if (y > maxY)
                maxY = y;
            else if (y < minY)
                minY = y;
        }
        
        Width = maxX - minX + 1;
        Height = maxY - minY + 1;
        
        int xOffset = -minX;
        int yOffset = -minY;
    }

    private static (int dx, int dy) DeltaFromInstruction(DigInstruction instruction)
    {
        int dx = instruction.Dir switch
        {
            Direction.Right => instruction.Distance,
            Direction.Left => -instruction.Distance,
            _ => 0
        };
        int dy = instruction.Dir switch
        {
            Direction.Down => instruction.Distance,
            Direction.Up => -instruction.Distance,
            _ => 0
        };
        return (dx, dy);
    }
}

internal sealed record DigInstruction(Direction Dir, ushort Distance, string Color);
internal sealed record Line(int StartX, int StartY, int EndX, int EndY);