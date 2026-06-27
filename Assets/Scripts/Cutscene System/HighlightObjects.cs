using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObjects : MonoBehaviour
{
    public bool Finished = false;
    public List<GameObject> gameObjects = new List<GameObject>();
    float growTime = 0.8f;
    float waitTime = 2.0f;
    Color hightlightColor = Color.green;

    public void Highlight()
    {
        StartCoroutine(HighlightCoroutine());
    }

    IEnumerator HighlightCoroutine()
    {
        float timer = 0.0f;
        Color baseColor = gameObjects[0].GetComponentInChildren<Renderer>().material.color;
            
        while (timer < growTime)
        {
            timer += Time.deltaTime;
            float t = timer / growTime;
    
            foreach (GameObject obj in gameObjects)
            {
                obj.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.2f, 1.2f, 1.2f), t);
                float red = Mathf.Lerp(baseColor.r, hightlightColor.r, t);
                float green = Mathf.Lerp(baseColor.g, hightlightColor.g, t);
                float blue = Mathf.Lerp(baseColor.b, hightlightColor.b, t);
                obj.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", new Color(red, green, blue));
            }
            yield return null;
        }
        float waitTimer = 0.0f;
        Finished = true;
        while (waitTimer < waitTime)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }
        while (timer > 0.0f)
        {
            timer -= Time.deltaTime;
            float t = timer / growTime;
    
            foreach (GameObject obj in gameObjects)
            {
                obj.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.2f, 1.2f, 1.2f), t);
                float red = Mathf.Lerp(baseColor.r, hightlightColor.r, t);
                float green = Mathf.Lerp(baseColor.g, hightlightColor.g, t);
                float blue = Mathf.Lerp(baseColor.b, hightlightColor.b, t);
                obj.GetComponentInChildren<Renderer>().material.SetColor("_BaseColor", new Color(red, green, blue));
            }
            yield return null;
        }

        yield return null;
    }
}
