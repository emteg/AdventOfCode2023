namespace ConsoleApp19;

internal sealed record CompareAndRejectRule : ComparePartRatingRule
{
    public CompareAndRejectRule(PartRating ratingToCompare, bool acceptWhenLessThan, uint threshold) 
        : base(ratingToCompare, acceptWhenLessThan, threshold)
    { }

    protected override void PartMatches(Part part) => part.Reject();
}