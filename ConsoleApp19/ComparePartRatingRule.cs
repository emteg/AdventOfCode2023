namespace ConsoleApp19;

internal abstract record ComparePartRatingRule : Rule
{
    public PartRating RatingToCompare { get; }

    public bool AcceptWhenLessThan { get; }

    public uint Threshold { get; }
    
    public override bool Apply(Part part)
    {
        uint rating = part.Rating(RatingToCompare);
        
        if (AcceptWhenLessThan && rating < Threshold)
        {
            PartMatches(part);
            return true;
        }
        
        if (!AcceptWhenLessThan && rating > Threshold)
        {
            PartMatches(part);
            return true;
        }

        return false;
    }

    protected ComparePartRatingRule(PartRating ratingToCompare, bool acceptWhenLessThan, uint threshold)
    {
        RatingToCompare = ratingToCompare;
        AcceptWhenLessThan = acceptWhenLessThan;
        Threshold = threshold;
    }

    protected abstract void PartMatches(Part part);
}