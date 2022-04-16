using System.Collections.Generic;

public class PatternLib<T> where T: struct
{
    public int Range;
    public List<Pattern<T>> Patterns;

    public List<T> SupportedValues;
    public int Count => Patterns.Count;
    public int ValuesCount => SupportedValues.Count;

    public List<int> SupportedValuesWeights = new List<int>();

    public int CoreValId(int patternId) => SupportedValues.FindIndex(v => v.Equals(Patterns[patternId].CoreValue));

    public void GeneratePatterns(ref T[,] data, int range, bool flipandrot = true)
    {
        Range = range;
        var patterns = new List<Pattern<T>>();
        for (int i = range; i < data.GetLength(0) - range; i++)
        {
            for (int k = range; k < data.GetLength(1) - range; k++)
            {
                AddStraight(data, range, i, k, patterns);
            }
        }

        FillUniquePatterns(patterns);
        FillUniqueValues();
    }

    private static void AddStraight(
        T[,] data,
        int range,
        int i,
        int k,
        List<Pattern<T>> patterns)
    {
        var pattern = new Pattern<T>(range);
        for (int x = 0; x < pattern.W; x++)
        {
            for (int y = 0; y < pattern.W; y++)
            {
                pattern.data[x, y] = data[i - range + x, k - range + y];
            }
        }
        patterns.Add(pattern);
    }

    private void FillUniqueValues()
    {
        SupportedValues = new List<T>();
        foreach (var pattern in Patterns)
        {
            if (!SupportedValues.Contains(pattern.CoreValue))
            {
                SupportedValues.Add(pattern.CoreValue);
                SupportedValuesWeights.Add(1);
            }
            else
            {
                var idx = SupportedValues.FindIndex(p=>p.Equals(pattern.CoreValue));
                SupportedValuesWeights[idx]++;
            }
        }
    }


    private void FillUniquePatterns(List<Pattern<T>> patterns)
    {
        Patterns = new List<Pattern<T>>();
        while (patterns.Count > 0)
        {
            int occurencies = 1;
            var newpat = patterns[0];
            patterns.RemoveAt(0);

            for (int i = patterns.Count - 1; i >= 0; i--)
            {
                if (newpat.Match(patterns[i]))
                {
                    occurencies++;
                    patterns.RemoveAt(i);
                }
            }
            newpat.Occurecies = occurencies;
            Patterns.Add(newpat);
        }
    }
}