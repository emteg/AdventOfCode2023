namespace ConsoleApp19;

internal abstract record ComparePartRatingRule(PartRating RatingToCompare, bool AcceptWhenLessThan, uint Threshold) : Rule
{
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

    protected abstract void PartMatches(Part part);
}