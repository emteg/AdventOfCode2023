// See https://aka.ms/new-console-template for more information

using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        // Part 1
        StringReader sampleInput1 = new(/*@"467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598.."
/*
@"12.......*..
+.........34
.......-12..
..78........
..*....60...
78.........9
.5.....23..$
8...90*12...
............
2.2......12.
.*.........*
1.1..503+.56"
/*
@".......5......
..7*..*.....4*
...*13*......9
.......15.....
..............
..............
..............
..............
..............
..............
21............
...*9........."
/*
@".......5......
..7*..*.......
...*13*.......
.......15....."*/
/*
@"....................
..-52..52-..52..52..
..................-."*/
@"......*635
334.......");

        StringReader sampleInput2 = new(@"467..114..
...*......
..35..633.
......#...
617*......
.....+.58.
..592.....
......755.
...$.*....
.664.598..");

        //StringReader input = sampleInput2;
        StreamReader input = File.OpenText("input.txt");

        NumberToken? lastNumber = null;
        List<NumberToken> numbers = new();
        List<(uint X, uint Y, char C)> symbols = new();
        
        uint y = 0;
        foreach (string line in input.EnumerateLines())
        {
            uint x = 0;
            foreach (char c in line)
            {
                if (c.IsNumber())
                {
                    if (lastNumber is null)
                    {
                        lastNumber = new NumberToken(x, y, c);
                        numbers.Add(lastNumber);
                    }
                    else
                    {
                        lastNumber.AddChar(c);
                    }
                }
                // Part 1
                /*else if (c != '.' && c.IsSymbol())
                {
                    symbols.Add((x, y, c));
                    lastNumber?.Finish();
                    lastNumber = null;
                }
                else if (c == '.')
                {
                    lastNumber?.Finish();
                    lastNumber = null;
                }*/
                // Part 2
                else if (c == '*')
                {
                    symbols.Add((x, y, c));
                    lastNumber?.Finish();
                    lastNumber = null;
                }
                else
                {
                    lastNumber?.Finish();
                    lastNumber = null;
                }
                x++;
            }
            lastNumber?.Finish();
            lastNumber = null;
            y++;
        }

        uint result = 0;
        HashSet<NumberToken> tokens = new();
        foreach ((uint x, uint line, char c) in symbols)
        {
            // Part 1
            /*foreach (NumberToken number in numbers.Where(it => it.IsAdjacentToSymbolAt(x, line)))
            {
                if (tokens.Add(number))
                    result += number.Value;
            }*/
            
            // Part 2
            List<NumberToken> numbersAroundSymbol = numbers.Where(it => it.IsAdjacentToSymbolAt(x, line)).ToList();
            if (numbersAroundSymbol.Count == 2)
                result += numbersAroundSymbol[0].Value * numbersAroundSymbol[1].Value;
        }
        
        Console.ResetColor();
        Console.WriteLine(result);
    }


    private record NumberToken
    {
        public readonly uint StartX;
        public readonly uint Y;
        public uint Length { get; private set; } = 1;
        public uint EndX => StartX + Length - 1;
        public uint Value { get; private set; } = 0;

        public NumberToken(uint startX, uint y, char c)
        {
            StartX = startX;
            Y = y;
            valueChars.Push(c);
        }

        public void AddChar(char c)
        {
            valueChars.Push(c);
            Length++;
        }

        public void Finish()
        {
            uint multiplier = 1;
            while (valueChars.TryPop(out char c))
            {
                Value += (uint)(c - 48) * multiplier;
                multiplier *= 10;
            }
        }
        
        public bool IsAdjacentToSymbolAt(uint x, uint y)
        {
            if (y < (long)Y - 1 || y > Y + 1)
                return false;
            if (x < (long)StartX - 1)
                return false;
            if (x > EndX + 1)
                return false;

            return true;
        }

        private readonly Stack<char> valueChars = new(3);
    }
}

