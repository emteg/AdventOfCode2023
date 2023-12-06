// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        StringReader sampleInput = new(@"Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53
Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19
Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1
Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83
Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36
Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11");

        var input = File.OpenText("input.txt");//sampleInput;

        int nextCardId = 1;
        List<Card> cards = new();
        foreach (string line in input.EnumerateLines()) 
            cards.Add(ParseCard(line, ref nextCardId));
        
        // Part 1
        //Console.WriteLine(cards.Sum(it => it.Value));
        
        // Part2
        for (int i = 0; i < cards.Count; i++)
        {
            Card card = cards[i];
            int cardsToCopy = card.ValueNumbers.Count();
            for (int j = i + 1; j < cards.Count && j <= i + cardsToCopy; j++)
            {
                cards[j].Multiplier += card.Multiplier;
            }
        }
        Console.WriteLine(cards.Sum(it => it.Multiplier));
    }

    private static Card ParseCard(string line, ref int nextCardId)
    {
        State state = State.SearchStart;
        int startOfCurrentNumber = -1;
        int length = 0;
        List<int> winningNumbers = new();
        List<int> numbers = new();
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            bool isNumber = c.IsNumber();

            state = (state, c, isNumber) switch
            {
                (State.SearchStart,         ':',     _)     => State.SearchWinningNumber,
                
                (State.SearchStart,         _,         _)     => state,
                (State.SearchWinningNumber, ' ',     _)     => state,
                (State.SearchCardNumber,    ' ',     _)     => state,
                (State.SearchWinningNumber, '|',     _)     => State.SearchCardNumber,
                
                (State.SearchWinningNumber, _, true)  => StartNumber(i, State.ReadWinningNumber),
                (State.SearchCardNumber,    _, true)  => StartNumber(i, State.ReadCardNumber),
                
                (State.ReadWinningNumber,   _, true)  => Continue(),
                (State.ReadCardNumber,      _, true)  => Continue(),
                
                (State.ReadWinningNumber,   _, false) => FinishNumber(winningNumbers, State.SearchWinningNumber),
                (State.ReadCardNumber,      _, false) => FinishNumber(numbers, State.SearchCardNumber)
            };
        }

        if (state is State.ReadCardNumber) // end of line
            FinishNumber(numbers, State.ReadCardNumber);

        return new Card(nextCardId++, winningNumbers, numbers);

        State FinishNumber(List<int> listToAddTo, State stateToChangeTo)
        {
            string currentNumber = line.Substring(startOfCurrentNumber, length);
            listToAddTo.Add(int.Parse(currentNumber));
            length = -1;
            startOfCurrentNumber = -1;
            return stateToChangeTo;
        }

        State StartNumber(int index, State stateToChangeTo)
        {
            startOfCurrentNumber = index;
            length = 1;
            return stateToChangeTo;
        }

        State Continue()
        {
            length++;
            return state;
        }
    }

    private record Card(int Id, List<int> WinningNumbers, List<int> Numbers)
    {
        public uint Multiplier = 1;
        public uint Value
        {
            get
            {
                if (WinningNumbers.Count == 0 || Numbers.Count == 0)
                    return 0;

                uint result = 0;
                foreach (int _ in ValueNumbers)
                {
                    if (result == 0)
                        result = 1;
                    else
                        result *= 2;
                }
                return result;
            }
        }

        public IEnumerable<int> ValueNumbers => Numbers.Where(it => WinningNumbers.Contains(it));

        public override string ToString()
        {
            return $"{Id}: {string.Join(", ", ValueNumbers)} | {ValueNumbers.Count()} | {Multiplier}";
        }
    }

    private enum State
    {
        SearchStart,
        ReadWinningNumber,
        SearchWinningNumber,
        SearchCardNumber,
        ReadCardNumber
    }
}