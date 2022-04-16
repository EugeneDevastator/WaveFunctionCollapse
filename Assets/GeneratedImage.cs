using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = System.Random;

public class GeneratedImage<T>
    where T : struct
{
    public T[,] Values;

    private int random_range = 4;

    //  public List<int>[,] PatternWeights;
    public bool[,] Mask;
    public bool[,] HasValue;
    public int[,] UsedPatID;
    public ProbbFlags[,] Probbs;

    private readonly WfcRenderPipeline<T> m_WfcRenderPipeline;
    private Random rnd;

    public int W { get; private set; }

    public int H { get; private set; }

    public WfcRenderPipeline<T> WfcRenderPipeline
    {
        get { return m_WfcRenderPipeline; }
    }

    public GeneratedImage(int w, int h, T defaultVal, PatternLib<T> patternLib)
    {
        W = w;
        H = h;
        rnd = new Random();
        Values = new T[w, h];

        //     PatternWeights = new List<int>[w,h];
        Mask = new bool[w, h];
        HasValue = new bool[w, h];
        UsedPatID = new int[w, h];
        Probbs = new ProbbFlags[w, h];

        ArrayExtensions.FillArray(ref Mask, false);
        ArrayExtensions.FillArray(ref HasValue, false);
        ArrayExtensions.FillArray(ref Values, defaultVal);
        ArrayExtensions.FillArray(ref UsedPatID, 0);
        ArrayExtensions.FillArray(ref Probbs, () => new ProbbFlags(patternLib.Count, patternLib.SupportedValuesWeights, rnd));
    }

    public void SetValue(int x, int y, int patId, PatternLib<T> lib)
    {
        Values[x, y] = lib.Patterns[patId].CoreValue;
        UsedPatID[x, y] = patId;
        Mask[x, y] = true;
        HasValue[x, y] = true;
    }

    public void SetValueOnly(int x, int y, T value)
    {
        Values[x, y] = value;
        Mask[x, y] = true;
        HasValue[x, y] = true;
    }

    public int UpdateAllProbsOnMask(PatternLib<T> lib)
    {
        int count = 0;
        var range = lib.Range;
        for (int x = lib.Range; x < Values.GetLength(0) - range; x++)
        {
            for (int y = lib.Range; y < Values.GetLength(1) - range; y++)
            {
                if (Mask[x, y] && !HasValue[x, y])
                {
                    count = count + UpdateProbbsAt(x, y, lib);
                }
            }
        }
        Debug.Log(Time.frameCount + $"GeneratedImage.cs: Prob update count: {count}");
        return count;
    }

    public int UpdateProbbsAt(int x, int y, PatternLib<T> patternLib)
    {
        int count = 0;
        for (int patid = 0; patid < patternLib.Count; patid++)
        {
            var res = UpdateProbbForPatternAt(x, y, patternLib, patid);
            count = count + (res ? 1 : 0);
        }
        return count;
    }

    private void BanPatternAt(int x, int y, int patId, PatternLib<T> lib)
    {
        Probbs[x, y].RemoveProb(patId, lib.CoreValId(patId));
        for (int i = 0; i < lib.Range * 2 + 1; i++)
        {
            for (int k = 0; k < lib.Range * 2 + 1; k++)
            {
                {
                    var dataXo = x - lib.Range + i;
                    var dataYo = y - lib.Range + k;
                }
            }
        }
    }

    private bool UpdateProbbForPatternAt(
        int x,
        int y,
        PatternLib<T> lib,
        int patid)
    {
        if (!Probbs[x, y].CanBe[patid])
        {
            return false;
        }

        Pattern<T> pat = lib.Patterns[patid];
        int patternRng = pat.Range;
        for (int i = 0; i < patternRng * 2 + 1; i++)
        {
            for (int k = 0; k < patternRng * 2 + 1; k++)
            {
                var dataXo = x - patternRng + i;
                var dataYo = y - patternRng + k;
                if (HasValue[dataXo, dataYo])
                {
                    if (!pat.data[i, k].Equals(Values[dataXo, dataYo]))
                    {
                        Probbs[x, y].RemoveProb(patid, lib.CoreValId(patid));
                        return true;
                    }
                }
                else if (Probbs[dataXo, dataYo].entropy > 0)// remove inversely for impossible values;
                {
                    foreach (var impossibleId in Probbs[dataXo, dataYo].ImpossibleValIds)
                    {
                        var impVal = lib.SupportedValues[impossibleId];
                        if (pat.data[i, k].Equals(impVal))
                        {
                            Probbs[x, y].RemoveProb(patid, lib.CoreValId(patid));
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

/*
    public void UpdateAllCellsFast(PatternLib<T> patternLib)
    {
        for (int i = 0; i < W; i++)
        {
            for (int k = 0; k < H; k++)
            {
                UpdateCellDataFast(i, k, patternLib);
            }
        }
    }
*/
    public void CollapseOneMinCell(PatternLib<T> lib)
    {
        int count = 0;
        int minEntro = lib.Count * 2;
        int maxNears = 1;

        (int x, int y) best = (-1, -1);
        int scanned = 0;

        int range = lib.Range;
        for (int x = range; x < Values.GetLength(0) - range; x++)
        {
            for (int y = range; y < Values.GetLength(1) - range; y++)
            {
                if (!HasValue[x, y] && Mask[x, y])
                {
                    if (best.x < 0)
                    {
                        best = (x, y);
                    }

                    scanned++;

                    //UpdateNeighbors(lib, x, y, out var nearW);

                    int entropy = Probbs[x, y].entropy;
                    if (entropy < minEntro && entropy > 0)
                    {
                        minEntro = entropy;
                        count++;
                        best = (x, y);
                    }

                    /*    if (nearW > maxNears)
                        {
                            best = (x, y);
                            minEntro = entropy;
                            maxNears = nearW;
                        }
                        else if (nearW == maxNears && entropy < minEntro)
                        {
                            best = (x, y);
                            minEntro = entropy;
                            maxNears = nearW;
                        }*/
                }
            }
        }

        if (count < 1)
        {
            Debug.Log(Time.frameCount + $"No godd cells");
        }
        else
        {
            Debug.Log(Time.frameCount + $"GeneratedImage.cs: minEntro at collapse: {minEntro}");
            TryCollapse(best.x, best.y, lib);
        }

        void UpdateNeighbors(PatternLib<T> lib, int x, int y, out int nearW)
        {
            nearW = 0;
            for (int i = 0; i < lib.Range * 2 + 1; i++)
            {
                for (int k = 0; k < lib.Range * 2 + 1; k++)
                {
                    nearW += HasValue[x - lib.Range + i, y - lib.Range + k] ? 1 : 0;
                }
            }
        }
    }

    private int GetEntropy(int x, int y, PatternLib<T> lib)
    {
        int max = 0;
        int min = 90000000;
        for (int pid = 0; pid < Probbs[x,y].CanBe.Length; pid++)
        {
            if (Probbs[x, y].CanBe[pid])
            {
                var val = lib.Patterns[pid].Occurecies;
                if (val > max) max = val;
                if (val < min) min = val;
            }
        }

        return -(max - min);
    }

    private void TryCollapse(int x, int y, PatternLib<T> lib)
    {
        if (Probbs[x, y].entropy < 1)
        {
            RewindImpossible(x, y, lib);
            //CollapseToPureRandom(x, y, lib);
        }
        else
        {
            CollapseByPatternProb(x, y, lib);
        }
    }

    private void CollapseToPureRandom(int x, int y, PatternLib<T> lib)
    {
        int finalId = rnd.Next(0, lib.Count);
        SetValue(x, y, finalId, lib);
    }

    private void CollapseByPatternProb(int x, int y, PatternLib<T> lib)
    {
        var indexDistrib = new List<int>();
        for (int i = 0; i < Probbs[x, y].CanBe.Length; i++)
        {
            if (Probbs[x, y].CanBe[i])
                for (int k = 0; k < lib.Patterns[i].Occurecies; k++)
                {
                    indexDistrib.Add(i);
                }
        }
        int distId = rnd.Next(0, indexDistrib.Count);
        int finalId = indexDistrib[distId];
        SetValue(x, y, finalId, lib);
    }

    private void CollapseByValProb(int x, int y, PatternLib<T> lib)
    {
        var indexDistrib = new List<int>();
        for (int i = 0; i < Probbs[x, y].WeightPerValue.Length; i++)
        {
            for (int k = 0; k < Probbs[x, y].WeightPerValue[i]; k++)
            {
                indexDistrib.Add(i);
            }
        }

        var selId = indexDistrib[rnd.Next(0, indexDistrib.Count)];
        var val = lib.SupportedValues[selId];
        SetValueOnly(x, y, val);
    }

    private void SlapRandomPattern(int x, int y, PatternLib<T> lib)
    {
        int finalId = rnd.Next(0, lib.Count);
        var pat = lib.Patterns[finalId];

        for (int i = 0; i < lib.Range * 2 + 1; i++)
        {
            for (int k = 0; k < lib.Range * 2 + 1; k++)
            {
                var ox = x - lib.Range + i;
                var oy = y - lib.Range + k;
                SetValueOnly(x, y, pat.data[i, k]);
            }
        }
    }

    private void RewindImpossible(int x, int y, PatternLib<T> lib)
    {
        int range = 1;
        for (int i = 0; i < range * 2 + 1; i++)
        {
            for (int k = 0; k < range * 2 + 1; k++)
            {
                var ox = x - range + i;
                var oy = y - range + k;

                if (HasValue[ox, oy])
                {
                    Probbs[ox, oy].RemoveProb(UsedPatID[ox, oy], lib.CoreValId(UsedPatID[ox, oy]));
                    HasValue[ox, oy] = false;
                    Mask[ox, oy] = true;
                }
                else
                {
                    Probbs[ox, oy] = new ProbbFlags(lib.Count, lib.SupportedValuesWeights, rnd);
                    Mask[ox, oy] = true;
                }
            }
        }
    }

    public void WriteToRT(RenderTexture renderTex, Func<T, Color> converter)
    {
        Texture2D texture = new Texture2D(W, H);
        for (int i = 0; i < W; i++)
        {
            for (int k = 0; k < H; k++)
            {
                Color col = Color.blue;
                if (Mask[i, k])
                {
                    if (!HasValue[i, k])
                    {
                        col = Color.magenta * Probbs[i, k].entropy / Probbs[i, k].CanBe.Length + new Color(0, 0, 0.1f, 1f);
                    }
                    else
                    {
                        col = converter(Values[i, k]); // > 0 ? Color.white : Color.black;
                    }
                }
                texture.SetPixel(i, k, col);
            }
        }
        texture.Apply();
        Graphics.Blit(texture, renderTex);
    }

    public void ExpandMask(int range = 1)
    {
        for (int i = range; i < W - range; i++)
        {
            for (int k = range; k < H - range; k++)
            {
                if (HasValue[i, k])
                {
                    for (int x = 0; x < range * 2 + 1; x++)
                    {
                        for (int y = 0; y < range * 2 + 1; y++)
                        {
                            Mask[i - range + x, k - range + y] = true;
                        }
                    }
                }
            }
        }
    }

    private static (int patId, int) WightFormula(int patId, int weight, Pattern<T> pattern) => (patId, weight * 1);
}