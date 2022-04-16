using System;

public struct Pattern<TValue> where TValue: struct
{
    public readonly int Range;

    public readonly int W;

    public int Occurecies;

    public TValue[,] data;

    public TValue CoreValue => data[Range, Range];

    public Pattern(int range)
    {
        Range = range;
        Occurecies = 0;
        W = range * 2 + 1;
        var w = range * 2 + 1;
        this.data = new TValue[w, w];
    }

    /// <summary>
    /// Compares two patterns.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Match(Pattern<TValue> other)
    {
        if (other.Range != Range)
        {
            return false;
        }

        for (int i = 0; i < W; i++)
        {
            for (int k = 0; k < W; k++)
            {
                if (!data[i, k].Equals(other.data[i, k]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public readonly bool MatchLocation(ref TValue[,] otherData, int ox, int oy)
    {
        for (int i = 0; i < W; i++)
        {
            for (int k = 0; k < W; k++)
            {
                if (!data[i, k].Equals(otherData[ox + i, oy + k]))
                {
                    return false;
                }
            }
        }
        return true;
    }
}