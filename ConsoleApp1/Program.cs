// See https://aka.ms/new-console-template for more information

using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Console.WriteLine(Part1(new StringReader(@"1abc2
pqr3stu8vwx
a1b2c3d4e5f
treb7uchet")));
        Console.WriteLine(Part1(File.OpenText("puzzleInput1.txt")));
        Console.WriteLine(Part2(new StringReader(@"two1nine
eightwothree
abcone2threexyz
xtwone3four
4nineeightseven2
zoneight234
7pqrstsixteen")));
        Console.WriteLine(Part2(File.OpenText("puzzleInput1.txt")));
    }

    private static uint Part1(TextReader reader) => Execute(reader, ExtractDigits);

    private static uint Part2(TextReader reader) => Execute(reader, ExtractDigitsFromWords);

    private static uint Execute(TextReader reader, Func<string, IEnumerable<uint>> extract)
    {
        return reader.EnumerateLines()
            .Select(extract)
            .Select(NumberFromFirstAndLastDigit)
            .Sum();
    }

    private static IEnumerable<uint> ExtractDigits(string s) 
        => s
            .Where(CharIsDigit)
            .Select(CharToUint);

    private static uint CharToUint(char c) => (uint)c - 48;

    private static bool CharIsDigit(char c) => c > 48 && c <= 57;

    private static IEnumerable<uint> ExtractDigitsFromWords(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (CharIsDigit(c))
            {
                yield return CharToUint(c);
                continue;
            }
            foreach ((string word, uint value) in Words)
            {
                if (line.IndexOf(word, i, StringComparison.InvariantCultureIgnoreCase) != i) 
                    continue;
                
                yield return value;
                break;
            }
        }
    }

    private static uint NumberFromFirstAndLastDigit(IEnumerable<uint> digits) => 10 * digits.First() + digits.Last();

    

    private static readonly (string word, uint value)[] Words =
    {
        ("one", 1),
        ("two", 2),
        ("three", 3),
        ("four", 4),
        ("five", 5),
        ("six", 6),
        ("seven", 7),
        ("eight", 8),
        ("nine", 9),
    };
}