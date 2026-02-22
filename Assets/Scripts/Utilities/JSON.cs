using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using Utilities.ML;

namespace Utilities
{
    public class JSON
    {
        [Serializable]
        public struct MatrixJSON
        {
            public int rows;
            public int columns;
            public double[] data;

            public MatrixJSON(int rows, int columns, double[] data)
            {
                this.rows = rows;
                this.columns = columns;
                this.data = data;
            }
            public MatrixJSON(Matrix matrix)
            {
                rows = matrix.Rows;
                columns = matrix.Columns;
                data = matrix.Data;
            }
            public Matrix ToMatrix()
            {
                Matrix res = new Matrix(rows,columns);
                for (int i = 0; i < data.Length; i++)
                    res[i] = data[i];
                return res;
            }
        }

        [Serializable]
        public struct LayerJSON
        {
            public MatrixJSON weights;
            public MatrixJSON biases;

            public LayerJSON(Matrix weights, Matrix biases)
            {
                this.weights = new MatrixJSON(weights);
                this.biases = new MatrixJSON(biases);
            }
        }

        [Serializable]
        public struct NetworkJSON
        {
            public List<LayerJSON> layerJSONs;

            public NetworkJSON(FullyConnectedNN network)
            {
                layerJSONs = new List<LayerJSON>();
                foreach (Layer l in network.layers)
                    layerJSONs.Add(new LayerJSON(l.weights, l.biases));
            }
        }
        
        public static string MatrixToJSON(Matrix matrix)
        {
            MatrixJSON obj = new MatrixJSON(matrix);
            return JsonUtility.ToJson(obj);
        }

        public static Matrix JSONToMatrix(string json)
        {
            MatrixJSON obj = JsonUtility.FromJson<MatrixJSON>(json);
            return obj.ToMatrix();
        }

        public static string LayerToJSON(Layer layer)
        {
            LayerJSON obj = new LayerJSON(layer.weights, layer.biases);
            return JsonUtility.ToJson(obj);
        }

        public static LayerJSON JSONToLayer(string json)
        {
            LayerJSON obj = JsonUtility.FromJson<LayerJSON>(json);
            return obj;
        }

        public static string NetworkToJSON(FullyConnectedNN network)
        {
            NetworkJSON obj = new NetworkJSON(network);
            return JsonUtility.ToJson(obj);
        }

        public static NetworkJSON JSONToNetwork(string json)
        {
            NetworkJSON obj = JsonUtility.FromJson<NetworkJSON>(json);
            return obj;
        }

        public static void SaveJSONFile(string path, string jsonObject)
        {
            string destination = Application.persistentDataPath + path;
            FileStream file;

            if (File.Exists(destination)) file = File.OpenWrite(destination);
            else
            {
                FileInfo fi = new FileInfo(destination);
                if (!fi.Directory.Exists) Directory.CreateDirectory(fi.DirectoryName);
                file = File.Create(destination);
            }

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, jsonObject);
            file.Close();
        }

        public static string LoadJSONFile(string path)
        {
            string destination = Application.persistentDataPath + path;
            FileStream file;

            if (File.Exists(destination)) file = File.OpenRead(destination);
            else return "";

            BinaryFormatter bf = new BinaryFormatter();
            string data = (string) bf.Deserialize(file);
            file.Close();

            return data;
        }

        public static string GetJSONPath(string name, int level)
        {
            return $"/lvl_{level}/{name.Replace(' ', '_')}.json";
        }

        public static string GetFileName(string path)
        {
            string bareName = Path.GetFileNameWithoutExtension(path);
            return bareName.Replace('_', ' ');
        }
    }
}