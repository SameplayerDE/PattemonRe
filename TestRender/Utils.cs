using System;
using System.IO;

namespace TestRender;

public class Utils
{
    public static string[,] ReadMatrix(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception("");
        }
        var lines = File.ReadAllLines(path);
        var rows = lines.Length;
        var cols = lines[0].Split(',').Length;

        var matrix = new string[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            var values = lines[i].Split(',');
            for (var j = 0; j < cols; j++)
            {
                matrix[i, j] = string.IsNullOrEmpty(values[j]) ? "" : values[j];
            }
        }

        return matrix;
    }
}