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

    public static int GreatestCommonDivisor(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    public static int LeastCommonMultiple(int a, int b)
        => a / GreatestCommonDivisor(a, b) * b;
    
    public static int LeastCommonMultiple(this IEnumerable<int> values)
        => values.Aggregate(LeastCommonMultiple);
    
    public static long GreatestCommonDivisor(long a, long b)
    {
        while (b != 0)
        {
            long temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    public static long LeastCommonMultiple(long a, long b)
        => a / GreatestCommonDivisor(a, b) * b;
    
    public static long LeastCommonMultiple(this IEnumerable<long> values)
        => values.Aggregate(LeastCommonMultiple);
    
    public static uint GreatestCommonDivisor(uint a, uint b)
    {
        while (b != 0)
        {
            uint temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    public static uint LeastCommonMultiple(uint a, uint b)
        => a / GreatestCommonDivisor(a, b) * b;
    
    public static uint LeastCommonMultiple(this IEnumerable<uint> values)
        => values.Aggregate(LeastCommonMultiple);
    
    public static ulong GreatestCommonDivisor(ulong a, ulong b)
    {
        while (b != 0)
        {
            ulong temp = b;
            b = a % b;
            a = temp;
        }

        return a;
    }

    public static ulong LeastCommonMultiple(ulong a, ulong b)
        => a / GreatestCommonDivisor(a, b) * b;
    
    public static ulong LeastCommonMultiple(this IEnumerable<ulong> values)
        => values.Aggregate(LeastCommonMultiple);
    
    public static ushort GreatestCommonDivisor(ushort a, ushort b)
    {
        while (b != 0)
        {
            ushort temp = b;
            b = (ushort)(a % b);
            a = temp;
        }

        return a;
    }

    public static ushort LeastCommonMultiple(ushort a, ushort b)
        => (ushort)(a / GreatestCommonDivisor(a, b) * b);
    
    public static ushort LeastCommonMultiple(this IEnumerable<ushort> values)
        => values.Aggregate(LeastCommonMultiple);
    
    public static short GreatestCommonDivisor(short a, short b)
    {
        while (b != 0)
        {
            short temp = b;
            b = (short)(a % b);
            a = temp;
        }

        return a;
    }

    public static short LeastCommonMultiple(short a, short b)
        => (short)(a / GreatestCommonDivisor(a, b) * b);
    
    public static short LeastCommonMultiple(this IEnumerable<short> values)
        => values.Aggregate(LeastCommonMultiple);
}