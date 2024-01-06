using System.Diagnostics;
using Util;

internal static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sample = @"19, 13, 30 @ -2,  1, -2
18, 19, 22 @ -1, -1, -2
20, 25, 34 @ -2, -2, -4
12, 31, 28 @ -1, -2, -1
20, 19, 15 @  1, -5, -3";

        //Part1(new StringReader(sample).EnumerateLines(), 7, 27);
        Part1(File.OpenText("input.txt").EnumerateLines(), 200000000000000, 400000000000000);
    }

    private static void Part1(IEnumerable<string> input, double low, double high)
    {
        List<Hailstone> hailstones = input
            .Select(Hailstone.FromString)
            .ToList();

        int intersections = FindIntersections(hailstones, low, high)
            .Count(it => it.doesIntersect);
        Console.WriteLine($"There are {intersections} hailstones that will intersect inside the target area in the future.");
    }

    private static IEnumerable<(Hailstone a, Hailstone b, bool doesIntersect, double x, double y)> 
        FindIntersections(IReadOnlyList<Hailstone> hailstones, double low, double high)
    {
        for (int i = 0; i < hailstones.Count - 1; i++)
        {
            Console.WriteLine($"Comparing hailstone {i} with {hailstones.Count - i - 1} others");
            for (int j = i + 1; j < hailstones.Count; j++)
            {
                Hailstone a = hailstones[i];
                Hailstone b = hailstones[j];
                (bool doesIntersect, double x, double y) = FindIntersection(a, b, low, high);

                if (!doesIntersect)
                {
                    yield return (a, b, false, x, y);
                    continue;
                }
                
                double ta = a.TimeAtLocation(x, y);
                double tb = b.TimeAtLocation(x, y);
                
                if (ta < 0 || tb < 0)
                {
                    yield return (a, b, false, x, y);
                    continue;
                }
                
                yield return (a, b, true, x, y);
            }
        }
    }

    private static (bool doesIntersect, double x, double y) FindIntersection(
        Hailstone a, Hailstone b, double low, double high)
    {
        LineEq fa = new(a.X0, a.Y0, a.X0 + a.Vx, a.Y0 + a.Vy);
        LineEq fb = new(b.X0, b.Y0, b.X0 + b.Vx, b.Y0 + b.Vy);
        
        // c1 * x + c2 =!= c3 * x + c4
        // x * (c1 - c3) = c4 - c2
        // x = (c4 - c2) / (c1 - c3)
        double x = (fb.C2 - fa.C2) / (fa.C1 - fb.C1);
        double y = fa.Y(x);

        return (low <= x && x <= high && low <= y && y <= high, x, y);
    }
}

[DebuggerDisplay("y = {C1} * x + {C2}")]
internal readonly struct LineEq
{
    public readonly double C1;
    public readonly double C2;

    public LineEq(double x0, double y0, double x1, double y1)
    {
        // 1) f(x0) = c1 * x0 + c2 = y0
        // 2) f(x1) = c1 * x1 + c2 = y1
        
        // => c2 = y1 - c1 * x1
        
        // c2 in 1) c1 * x0 + y1 - c1 * x1 = y0
        //             c1 * (x0 - x1) + y1 = y0
        
        // => c1 = (y0 - y1) / (x0 - x1)
        
        C1 = (y0 - y1) / (x0 - x1);
        C2 = y1 - C1 * x1;
    }

    public double Y(double x) => C1 * x + C2;
}

[DebuggerDisplay("{X0}, {Y0}, {Z0} @ {Vx}, {Vy}, {Vz}")]
internal readonly struct Hailstone
{
    public readonly double X0;
    public readonly double Y0;
    public readonly double Z0;
    public readonly double Vx;
    public readonly double Vy;
    public readonly double Vz;

    public Hailstone(double x0, double y0, double z0, double vx, double vy, double vz)
    {
        X0 = x0;
        Y0 = y0;
        Z0 = z0;
        Vx = vx;
        Vy = vy;
        Vz = vz;
    }

    public static Hailstone FromString(string s)
    {
        string[] parts = s.Split(" @ ");
        double[] loc = parts[0].Split(", ").Select(double.Parse).ToArray();
        double[] vel = parts[1].Split(", ").Select(double.Parse).ToArray();

        return new Hailstone(loc[0], loc[1], loc[2], vel[0], vel[1], vel[2]);
    }

    public double TimeAtLocation(double x, double y)
    {
        // x   X0     Vx | x - X0     Vx
        // y = Y0 + t Vy | y - Y0 = t Vy
        return (x - X0) / Vx;
    }
}