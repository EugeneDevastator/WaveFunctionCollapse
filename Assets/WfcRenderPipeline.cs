using UnityEngine;
using Random = System.Random;

public class WfcRenderPipeline<T>
    where T : struct
{
    private GeneratedImage<T> gen;
    private PatternLib<T> patternLib;

    public WfcRenderPipeline(GeneratedImage<T> gen, PatternLib<T> patternLib)
    {
        this.patternLib = patternLib;
        this.gen = gen;
        var rnd = new Random();
    }

    public void DoProbbsIteration(int treshold = 5)
    {
        gen.ExpandMask(2);
        int res = 1;
        while (res > 0){
            res = gen.UpdateAllProbsOnMask(patternLib);
        }
        Debug.Log(Time.frameCount + $"Collapsing...");

        gen.CollapseOneMinCell(patternLib);
        gen.ExpandMask(2);
        int a = 1;
    }

}