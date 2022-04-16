using System;

public static class ArrayExtensions
{
    public static void FillArray<T>(ref T[,] array, Func<T> factory)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int k = 0; k < array.GetLength(1); k++)
            {
                array[i, k] = factory.Invoke();
            }
        }
    }

    public static void FillArray<T>(ref T[,] array, T value) where T: struct
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int k = 0; k < array.GetLength(1); k++)
            {
                array[i, k] = value;
            }
        }
    }

    public static void IterXY<T>(this T[,] array, Action<int, int> iterator)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int k = 0; k < array.GetLength(1); k++)
            {
                iterator.Invoke(i, k);
            }
        }
    }
    public static void IterXYPad<T>(this T[,] array, int pad, Action<int, int> iterator)
    {
        for (int i = pad; i < array.GetLength(0)-pad; i++)
        {
            for (int k = pad; k < array.GetLength(1)-pad; k++)
            {
                iterator.Invoke(i, k);
            }
        }
    }


    public static T[,] UnPad<T>(this T[,] array, int xw, int yw)
    {
        T[,] newar = new T[array.GetLength(0) - xw * 2, array.GetLength(1) - yw * 2];
        newar.IterXY((x, y) => newar[x, y] = array[x + xw, y + yw]);
        return newar;
    }

}