using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public struct ProbbFlags
{
    // TODO: move everythin to account only for value probability.
    public int entropy;
    private Random rnd;

    public bool[] CanBe;
    public int[] WeightPerValue;
    public List<int> ImpossibleValIds;

    public ProbbFlags(int size, List<int> valWeights, Random rnd)
    {
        this.rnd = rnd;
        CanBe = new bool[size];
        for (int i = 0; i < size; i++)
        {
            CanBe[i] = true;
        }

        WeightPerValue = valWeights.ToArray();
        ImpossibleValIds = new List<int>();
        entropy = size;
    }

    public void RemoveProb(int idx, int valueId)
    {
        WeightPerValue[valueId] --;
        if (WeightPerValue[valueId] < 1)
        {
            ImpossibleValIds.Add(valueId);
        }
        CanBe[idx] = false;
        entropy--;
    }
}