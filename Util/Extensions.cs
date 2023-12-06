namespace Util;

public static class Extensions
{
    public static IEnumerable<string> EnumerateLines(this TextReader stream)
    {
        while (stream.ReadLine() is { } line)
            yield return line;
    }
    
    public static uint Sum(this IEnumerable<uint> enumerable)
    {
        return enumerable.Aggregate(0u, (total, current) => total + current);
    }

    public static bool IsNumber(this char c)
    {
        return c >= 48 && c <= 57;
    }

    public static bool IsSymbol(this char c)
    {
        if (c >= 33 && c <= 47)
            return true;
        if (c >= 58 && c <= 64)
            return true;
        if (c >= 91 && c <= 96)
            return true;
        if (c >= 123 && c <= 126)
            return true;
        
        return false;
    }
}