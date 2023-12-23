namespace ConsoleApp19;

internal sealed record CompareAndAcceptRule : ComparePartRatingRule
{
    public CompareAndAcceptRule(PartRating ratingToCompare, bool acceptWhenLessThan, uint threshold) 
        : base(ratingToCompare, acceptWhenLessThan, threshold)
    { }

    protected override void PartMatches(Part part) => part.Accept();
}