// See https://aka.ms/new-console-template for more information

using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        string sample = @"Time:      7  15   30
Distance:  9  40  200";

        string actual = @"Time:        40     82     84     92
Distance:   233   1011   1110   1487";

        string input = actual;
        
        Part1(new StringReader(input));
        Part2(new StringReader(input));
    }

    private static void Part2(TextReader input)
    {
        List<string> lines = input.EnumerateLines().ToList();
        ulong duration = lines[0].Split(':').Skip(1).Select(it => it.Replace(" ", "")).Select(ulong.Parse).First();
        ulong distance = lines[1].Split(':').Skip(1).Select(it => it.Replace(" ", "")).Select(ulong.Parse).First();
        Race race = new(duration, distance);

        (uint lower, uint upper) = race.ChargeTimeUpperAndLowerBound();
        Console.WriteLine(upper - lower -1);
    }

    private static void Part1(TextReader input)
    {
        List<string> lines = input.EnumerateLines().ToList();
        List<ulong> durations = lines[0].Split(' ').Skip(1).Where(it => it.Length > 0).Select(ulong.Parse).ToList();
        List<ulong> distances = lines[1].Split(' ').Skip(1).Where(it => it.Length > 0).Select(ulong.Parse).ToList();
        List<Race> races = new();
        for (int i = 0; i < durations.Count; i++)
        {
            ulong duration = durations[i];
            ulong distance = distances[i];
            races.Add(new Race(duration, distance));
        }

        Console.WriteLine(races
            .Select(race => race.ChargeTimeUpperAndLowerBound())
            .Select(bounds => bounds.upper - bounds.lower - 1)
            .Aggregate(1u, (total, current) => total * current));
    }

    private record Race(ulong Duration, ulong BestDistance)
    {
        public uint Acceleration = 1;
        
        public uint WaysToWin()
        {
            uint result = 0;
            
            for (ulong chargeDuration = 0; chargeDuration <= Duration; chargeDuration++)
            {
                ulong speed = chargeDuration * Acceleration;
                ulong distanceTravelled = (Duration - chargeDuration) * speed;
                if (distanceTravelled < BestDistance)
                    continue;
                result++;
            }

            return result;
        }

        /// <summary>
        /// Analytical solution:
        /// For a given race we know its total duration (T) and best distance travelled (d_best) so far.
        /// With the given total race duration T we can choose how much time (C) we want to spend to 'charge' our boat
        /// and this then leaves us with the remaining time (R) to travel as far as possible.
        /// 
        ///     T = C + R
        /// 
        /// When we charge longer, we travel faster, but we also have less time to travel. We want to travel at least
        /// 1 unit farther than the current record d_best, so we will be looking for upper and lower bounds of the
        /// charging time C.
        ///
        /// After we have been charging our boat for a time C, it will travel with a constant speed v
        /// 
        ///     v = a * C
        ///
        /// During the total race time, our boat will travel the following distance d over the remaining time R:
        /// 
        ///     d = v * R                   // insert v = a * c
        ///     d = a * C * R               // use R = T - C
        ///     d = a * C * (T - C)
        ///     d = a * C * T - a * C^2
        ///
        /// We are looking for a charging time C where d > d_best. To find the limit where this is true, we insert
        /// d_best for the distance travelled:
        ///
        ///     d_best =!= a * C * T - a * C^2
        /// 
        /// First, we reorder the equation to be equal to 0:
        /// 
        ///     d_best =!= a * C * T - a * C^2            // subtract d_best
        ///            0 = a * C * T - a * C^2 - d_best   // divide by a
        ///            0 = C * T - C^2 - d_best / a       // sort by degree of C
        ///            0 = -C^2 + C * T - d_best / a
        ///
        /// This equation is a polynomial of second degree: ax^2 + bx + c = 0. In our case, this means:
        /// 
        ///     a = -1              (from -C^2)
        ///     b = T               (from T * C)
        ///     c = -d_best / a
        ///
        /// Such a quadratic equation has 2 solutions x1 & x2 and we can solve it with this formula:
        /// 
        ///            -b +/- sqrt(b^2 - 4 * a * c)
        ///     x1,2 = ----------------------------
        ///                        2 * a
        /// </summary>
        public (uint lower, uint upper) ChargeTimeUpperAndLowerBound()
        {
            double a = -1;
            double b = Duration;
            double c = -(BestDistance / (double)Acceleration);
            
            double lowerBound = (-b + Math.Sqrt(Math.Pow(b, 2) - 4 * a * c)) / (2 * a);
            double upperBound = (-b - Math.Sqrt(Math.Pow(b, 2) - 4 * a * c)) / (2 * a);

            return ((uint)Math.Floor(lowerBound), (uint)Math.Ceiling(upperBound));
        }
    }
}