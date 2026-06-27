using System.Collections;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace Utilities
{
    public class GUI
    {
        public static TextMeshPro CreateWorldText(
            string text, Transform parent = null, Vector3 localPosition = default(Vector3),
            int fontSize = 40, Color? color = null, TextAlignmentOptions textAlignment = default(TextAlignmentOptions),
            int sortingOrder = 0)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(text, parent, localPosition, fontSize, (Color)color, textAlignment, sortingOrder);
        }

        public static void CreateWorldTextPopup(string text, Transform parent = null, Vector3 localPosition = default(Vector3),
            int fontSize = 40, Color? color = null, TextAlignmentOptions textAlignment = default(TextAlignmentOptions),
            int sortingOrder = 0)
        {
            // Create world text at position looking at camera
            if (color == null) color = Color.white;
            TextMeshPro popupText = CreateWorldText(text, parent, localPosition, fontSize, color, textAlignment, sortingOrder);

            popupText.gameObject.AddComponent<WorldPopup>();
        }


        public static TextMeshPro CreateWorldText(
            string text, Transform parent, Vector3 localPosition, int fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.alignment = textAlignment;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }

        private class WorldPopup : MonoBehaviour
        {
            float movespeed = 8f;
            void Start()
            {
                GameObject camera = GameObject.Find("Main Camera");

                var lookPos = camera.transform.position - transform.position;
                lookPos.y = 0;
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = rotation;
                transform.Rotate(Vector3.up * 180f);

                StartCoroutine(RemovePopup(1.2f));

                IEnumerator RemovePopup(float seconds)
                {
                    yield return new WaitForSeconds(seconds);
                    Destroy(gameObject);
                }
            }

            void Update()
            {
                transform.position += Vector3.up * movespeed * Time.deltaTime;
            }
        }
    }
}