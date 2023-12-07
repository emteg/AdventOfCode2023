using Util;

internal class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Tests();

        TextReader sampleInput = new StringReader(@"32T3K 765
T55J5 684
KK677 28
KTJJT 220
QQQJA 483");
        StreamReader actualInput = File.OpenText("input.txt");

        StreamReader input = actualInput;//sampleInput;

        (Hand, uint)[] ranked = input
            .EnumerateLines()
            .Select(line => line.Split(' '))
            .Select(arr => (arr[0].ToHand(), uint.Parse(arr[1])))
            .OrderBy(it => it.Item1)
            .ToArray();

        uint totalWinnings = 0;
        for (uint i = 0; i < ranked.Length; i++)
        {
            (Hand, uint) tuple = ranked[i];
            totalWinnings += (i + 1) * tuple.Item2;
        }
        Console.WriteLine(totalWinnings);
    }

    private static void Tests()
    {
        (string, Card[], HandType)[] handsInput =
        {
            ("33332", new [] {Card.Three, Card.Three, Card.Three, Card.Three, Card.Two}, HandType.FourOfAKind),
            ("2AAAA", new [] {Card.Two, Card.Ace, Card.Ace, Card.Ace, Card.Ace}, HandType.FourOfAKind),
            ("AAAAA", new [] {Card.Ace, Card.Ace, Card.Ace, Card.Ace, Card.Ace}, HandType.FiveOfAKind),
            ("23332", new [] {Card.Two, Card.Three, Card.Three, Card.Three, Card.Two}, HandType.FullHouse),
            ("AA8AA", new [] {Card.Ace, Card.Ace, Card.Eight, Card.Ace, Card.Ace}, HandType.FourOfAKind),
            ("TTT98", new [] {Card.Tee, Card.Tee, Card.Tee, Card.Nine, Card.Eight}, HandType.ThreeOfAKind),
            ("23432", new [] {Card.Two, Card.Three, Card.Four, Card.Three, Card.Two}, HandType.TwoPair),
            ("A23A4", new [] {Card.Ace, Card.Two, Card.Three, Card.Ace, Card.Four}, HandType.OnePair),
            ("23456", new [] {Card.Two, Card.Three, Card.Four, Card.Five, Card.Six}, HandType.HighCard) 
        };
        Card[][] hands = handsInput
            .Select(tuple => tuple.Item1.ToCardArray())
            .ToArray();
        Card[][] expectedHands = handsInput
            .Select(tuple => tuple.Item2)
            .ToArray();
        HandType[] expectedHandTypes = handsInput
            .Select(tuple => tuple.Item3)
            .ToArray();
        for (int i = 0; i < hands.Length; i++)
        {
            Card[] actualHand = hands[i];
            Card[] expectedHand = expectedHands[i];
            AssertEqual(actualHand, expectedHand);
            AssertEqual(actualHand.ToHandType(), expectedHandTypes[i]);
        }
        AssertTrue("AAAAA".ToHand() > "2AAAA".ToHand());
        AssertTrue("33332".ToHand() > "2AAAA".ToHand());
        AssertTrue("77888".ToHand() > "77788".ToHand());
    }

    private static void AssertEqual(Card expected, Card actual) 
    {
        if (expected != actual)
            throw new ApplicationException();
    }
    
    private static void AssertEqual(Card[] expected, Card[] actual)
    {
        if (expected.Length != actual.Length)
            throw new ApplicationException();

        for (int i = 0; i < expected.Length; i++) 
            AssertEqual(expected[i], actual[i]);
    }
    
    private static void AssertEqual(HandType expected, HandType actual) 
    {
        if (expected != actual)
            throw new ApplicationException();
    }

    private static void AssertTrue(bool boolean)
    {
        if (!boolean)
            throw new ApplicationException();
    }
}

public struct Hand : IComparable
{
    public Card[] Cards = new Card[5];
    public HandType Type = HandType.HighCard;

    public Hand() { }

    public static bool operator <(Hand a, Hand b) => a.CompareTo(b) < 0;

    public static bool operator >(Hand a, Hand b) => a.CompareTo(b) > 0;

    public static bool operator <=(Hand a, Hand b) => a.CompareTo(b) <= 0;

    public static bool operator >=(Hand a, Hand b) => a.CompareTo(b) >= 0;

    public static bool operator ==(Hand a, Hand b) => a.CompareTo(b) == 0;

    public static bool operator !=(Hand a, Hand b) => !(a == b);

    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;
        if (obj is not Hand otherHand)
            return -1;
        if (otherHand.Type != Type)
            return Type.CompareTo(otherHand.Type);

        for (int i = 0; i < Cards.Length; i++)
        {
            Card card = Cards[i];
            Card otherCard = otherHand.Cards[i];
            if (card != otherCard)
                return card.CompareTo(otherCard);
        }

        return 0;
    }
}

public enum HandType
{
    HighCard = 1,
    OnePair = 2,
    TwoPair = 3,
    ThreeOfAKind = 4,
    FullHouse = 5,
    FourOfAKind = 6,
    FiveOfAKind = 7
}

public enum Card
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Tee = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14,
}

public static class DaySevenExtensions {
    public static Card ToCard(this char c)
    {
        return c switch
        {
            '2' => Card.Two,
            '3' => Card.Three,
            '4' => Card.Four,
            '5' => Card.Five,
            '6' => Card.Six,
            '7' => Card.Seven,
            '8' => Card.Eight,
            '9' => Card.Nine,
            'T' => Card.Tee,
            'J' => Card.Jack,
            'Q' => Card.Queen,
            'K' => Card.King,
            'A' => Card.Ace,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static Card[] ToCardArray(this string s)
    {
        return s.ToCharArray().Select(c => c.ToCard()).ToArray();
    }

    public static HandType ToHandType(this Card[] cards)
    {
        Dictionary<Card, uint> counts = new();
        
        foreach (Card card in cards)
        {
            if (!counts.TryAdd(card, 1))
                counts[card] += 1;
        }

        if (counts.Any(it => it.Value == 5))
            return HandType.FiveOfAKind;
        if (counts.Any(it => it.Value == 4))
            return HandType.FourOfAKind;
        if (counts.Any(it => it.Value == 3) && counts.Any(it => it.Value == 2))
            return HandType.FullHouse;
        if (counts.Any(it => it.Value == 3))
            return HandType.ThreeOfAKind;
        if (counts.Count(it => it.Value == 2) == 2)
            return HandType.TwoPair;
        if (counts.Any(it => it.Value == 2))
            return HandType.OnePair;

        return HandType.HighCard;
    }

    public static Hand ToHand(this string s)
    {
        Card[] cards = s.ToCardArray();
        return new Hand {Cards = cards, Type = cards.ToHandType()};
    }
}