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

        public static RenderTexture GetPrefabPreview(Transform prefab)
        {
            // Spawn the prefab at origin in the prefab layer
            Transform _object = Object.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            _object.gameObject.layer = 6;
            var children = _object.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = 6;
            }

            // Turn on the camera and write to the render texture
            GameObject camera = GameObject.Find("Prefab Preview").transform.GetChild(0).gameObject;
            camera.SetActive(true);
            camera.GetComponent<Camera>().Render();
            camera.SetActive(false);

            // Remove spawned prefab
            _object.gameObject.SetActive(false);
            Object.Destroy(_object.gameObject);

            // Get RenderTexture and transform it to a Texture2D
            RenderTexture tex = Resources.Load<RenderTexture>("PrefabPreview");

            return tex;
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