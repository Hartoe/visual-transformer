using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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

    void Start()
    {
        targetPosition = transform.position;
        TimeTickSystem.OnTick += CheckMoved;
        changeColors = StartCoroutine(ShiftColors());
        red.increase = true;
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
            Debug.Log($"Set color to {red.value}, {green.value}, {blue.value}");
            yield return new WaitForSeconds(Time.deltaTime * changeSpeed);
        }
    }
}
