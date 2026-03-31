using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Utilities;
using Utilities.ML;

public class Intent : WorldItem
{
    public const int MAX_VALUE = 255;

    [SerializeField] float changeSpeed = 0.1f;

    private struct RGBColor
    {
        public int value;
        public bool increase;
    }

    private RGBColor red, green, blue;
    private Coroutine changeColors;
    [SerializeField] Renderer renderer;
    [SerializeField] string JSONPath;

    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;
        //changeColors = StartCoroutine(ShiftColors());
        red.increase = true;

        if (!string.IsNullOrEmpty(JSONPath))
        {
            string matrixJSON = JSON.LoadJSONFile(JSONPath);
            state = JSON.JSONToMatrix(matrixJSON);
        }

        renderer.material.SetColor("_BaseColor", GetColorFromMatrix(state));
    }

    private double[] Flatten(Matrix matrix)
    {
        double[] result = new double[matrix.Rows * matrix.Columns];
        
        int x = 0;
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[x++] = matrix[i,j];
            }
        }

        return result;
    }

    private double[] Normalize(double[] array)
    {
        double min = array.Min();
        double max = array.Max();
        double range = max - min;

        if (range == 0.0)
            return array.Select(_ => 0.0).ToArray();

        return array.Select(x => (x - min) / range).ToArray();
    }

    private Color GetColorFromMatrix(Matrix matrix)
    {
        // Flatten matrix to get a 1 dimensional array
        double[] flat = Flatten(matrix);

        // Normalize the array
        double[] norm = Normalize(flat);

        // Split the array in three equal parts (as close as possible)
        int size = norm.Length;
        int baseSize = size / 3;
        int remainder = size % 3;

        int redSize = baseSize + (remainder > 0 ? 1 : 0);
        int greenSize = baseSize + (remainder > 1 ? 1 : 0);
        int blueSize = baseSize;

        double[] redComponent = new double[redSize];
        double[] greenComponent = new double[greenSize];
        double[] blueComponent = new double[blueSize];

        Array.Copy(norm, 0, redComponent, 0, redSize);
        Array.Copy(norm, redSize, greenComponent, 0, greenSize);
        Array.Copy(norm, redSize + greenSize, blueComponent, 0, blueSize);

        // Calculate average of each of the three parts
        double red = redComponent.Average();
        double green = greenComponent.Average();
        double blue = blueComponent.Average();

        Debug.Log(matrix.ToString());
        Debug.Log($"{red}, {green}, {blue}");

        // Use averages as R G B in the final color
        return new Color((float)red, (float)green, (float)blue);
    }

    private IEnumerator ShiftColors()
    {
        while (true)
        {
            if (red.increase)
            {
                red.value++;
                if (red.value >= 255)
                {
                    red.value = 255;
                    red.increase = false;
                    green.increase = true;
                }
            } else
                red.value = Math.Max(red.value-1, 0);
            if (green.increase)
            {
                green.value++;
                if (green.value >= 255)
                {
                    green.value = 255;
                    green.increase = false;
                    blue.increase = true;
                }
            } else
                green.value = Math.Max(green.value-1, 0);
            if (blue.increase)
            {
                blue.value++;
                if (blue.value >= 255)
                {
                    blue.value = 255;
                    blue.increase = false;
                    red.increase = true;
                }
            } else
                blue.value = Math.Max(blue.value-1, 0);
            renderer.material.SetColor("_BaseColor", new Color(red.value/255.0f, green.value/255.0f, blue.value/255.0f));
            yield return new WaitForSeconds(Time.deltaTime * changeSpeed);
        }
    }
}
