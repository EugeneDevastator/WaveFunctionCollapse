using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Texture2D trainDataTex;
    [HideInInspector]
    public RenderTexture rt;
    private WfcRenderPipeline<Color> render;
    private WfcRenderPipeline<int> renderFromId;
    private int imgW;
    private int imgH;
    private GeneratedImage<Color> generatedImage;
    private GeneratedImage<int> generatedImageID;

    [SerializeField]
    private int extent = 1;

    private PatternLib<Color> patternLib;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState(123124);
        var w = trainDataTex.width;
        var h = trainDataTex.height;

        Color[,] trainData = new Color[w, h];

        for (int i = 0; i < w; i++)
        {
            for (int k = 0; k < h; k++)
            {
                trainData[i, k] = trainDataTex.GetPixel(i, k);
            }
        }

        patternLib = new PatternLib<Color>();

        int RANGE1 = extent;
        int RANGE2 = 1;
        imgW = 50;
        imgH = 50;

        patternLib.GeneratePatterns(ref trainData, RANGE1);
        Debug.Log(Time.frameCount + $"Main.cs: patternLib. valcount: {patternLib.SupportedValues.Count}");
        //CreateIndexImage(ref trainData, patternLib, out var idTrainData);


        /*var idTrainData2 = idTrainData.UnPad(2,2);
        var patternLibOnId = new PatternLib<int>();
        patternLibOnId.GeneratePatterns(ref idTrainData2, RANGE2);

        generatedImageID = new GeneratedImage<int>(imgW,imgH,0);
        generatedImageID.SetValue((int)(imgW*0.5f),(int)(imgW*0.5f),10, patternLibOnId);
        renderFromId = new WfcRenderPipeline<int>(generatedImageID, patternLibOnId);
*/

        generatedImage = new GeneratedImage<Color>(imgW,imgH,Color.black, patternLib);
        render = new WfcRenderPipeline<Color>(generatedImage, patternLib);
        generatedImage.SetValue((int)(imgW*0.5f),(int)(imgW*0.5f),10, patternLib);


        rt = new RenderTexture(imgW, imgH, 0);
        rt.enableRandomWrite = true;
        rt.Create();
        rt.filterMode = FilterMode.Point;
        GetComponent<Image>().material.mainTexture = rt;
        int a = 1;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            for (int i = 0; i < 10; i++)
            {
                render.DoProbbsIteration(5);
            }

            //GeneratedImage.WriteToRT(rt, (id) => patternLib.Patterns[id].CoreValue > 0 ? Color.white : Color.black);
            //generatedImage.WriteToRT(rt, (id) => id > 0 ? Color.white : Color.black);
            generatedImage.WriteToRT(rt, c => c);
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            for (int i = 0; i < 1; i++)
            {
                render.DoProbbsIteration(5);
            }

            //GeneratedImage.WriteToRT(rt, (id) => patternLib.Patterns[id].CoreValue > 0 ? Color.white : Color.black);
            //generatedImage.WriteToRT(rt, (id) => id > 0 ? Color.white : Color.black);
            generatedImage.WriteToRT(rt, c => c);
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            for (int i = 0; i < 40; i++)
            {
                render.DoProbbsIteration(45);
            }

            //generatedImageID.WriteToRT(rt, (id) => patternLib.Patterns[id].CoreValue > 0 ? Color.white : Color.black);
            //generatedImage.WriteToRT(rt, (id) => id > 0 ? Color.white : Color.black);
        }
    }

    private void CreateIndexImage(ref byte[,] data, PatternLib<byte> patternLib, out int[,] idImage)
    {
        idImage = new int[data.GetLength(0), data.GetLength(1)];

        var range = patternLib.Range;

        for (int i = range; i < data.GetLength(0) - range; i++)
        {
            for (int k = range; k < data.GetLength(1) - range; k++)
            {
                idImage[i, k] = FindPattern(ref data, patternLib, i - range, k - range);
            }
        }


    }

    private int FindPattern(ref byte[,] indata, PatternLib<byte> patternLib, int ox, int oy)
    {
        int pid = 0;
        foreach (var p in patternLib.Patterns)
        {
            if (p.MatchLocation(ref indata, ox, oy))
            {
                return pid;
            }
            pid++;
        }
        return pid;
    }
}