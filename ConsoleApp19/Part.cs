namespace ConsoleApp19;

internal sealed record Part(uint X, uint M, uint A, uint S)
{
    public bool Accepted { get; private set; }
    public bool Rejected { get; private set; }
    
    public static Part FromString(string line)
    {
        uint[] ratings = line
            .Substring(1, line.Length -2)
            .Split(',')
            .Select(str => str.Substring(2))
            .Select(uint.Parse)
            .ToArray();
        return new Part(ratings[0], ratings[1], ratings[2], ratings[3]);
    }

    public void Accept() => Accepted = true;
    public void Reject() => Rejected = true;

    public uint Rating(PartRating partRating)
    {
        return partRating switch
        {
            PartRating.X => X,
            PartRating.M => M,
            PartRating.A => A,
            PartRating.S => S,
            _ => throw new InvalidOperationException()
        };
    }

    public uint Sum() => X + M + A + S;
}