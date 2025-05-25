using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassikNet
{
    public partial class ClassicNet
    {
        // Save current state to JSON
        public void Save(string path)
        {
            try
            {
                var saveData = new
                {
                    layerInfo = this.layerInfo,
                    layerNeuron_connections = ConvertToJaggedArray(this.layerNeuron_connections),
                    layerNeurons_biases = this.layerNeurons_biases,
                    layerInputPosition = this.layerInputPosition,
                    layerOutputPosition_descriptor = this.layerOutputPosition_descriptor
                };

                string json = JsonSerializer.Serialize(saveData, GetJsonOptions());
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save failed! Reason: {ex.Message}");
            }
        }

        // Load from JSON file
        public static ClassicNet LoadFromFile(string path, ClassicNet classicNet = null)
        {
            try
            {
                string json = File.ReadAllText(path);
                var saveData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                if (classicNet is null)
                    classicNet = new ClassicNet();
                classicNet.layerInfo = JsonSerializer.Deserialize<int[]>(saveData["layerInfo"].ToString());
                classicNet.layerNeuron_connections = ConvertFromJaggedArray(JsonSerializer.Deserialize<float[][][]>(saveData["layerNeuron_connections"].ToString()));
                classicNet.layerNeurons_biases = JsonSerializer.Deserialize<float[][]>(saveData["layerNeurons_biases"].ToString());
                classicNet.layerInputPosition = JsonSerializer.Deserialize<Dictionary<string, int>>(saveData["layerInputPosition"].ToString());
                classicNet.layerOutputPosition_descriptor = JsonSerializer.Deserialize<Dictionary<int, string>>(saveData["layerOutputPosition_descriptor"].ToString());

                return classicNet;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load failed! Reason: {ex.Message}");
            }

            return null;
        }

        // Helper methods for jagged array conversion
        private static float[][][] ConvertToJaggedArray(float[][,] array)
        {
            var jagged = new float[array.Length][][];
            for (int i = 0; i < array.Length; i++)
            {
                jagged[i] = new float[array[i].GetLength(0)][];
                for (int j = 0; j < array[i].GetLength(0); j++)
                {
                    jagged[i][j] = new float[array[i].GetLength(1)];
                    for (int k = 0; k < array[i].GetLength(1); k++)
                    {
                        jagged[i][j][k] = array[i][j, k];
                    }
                }
            }
            return jagged;
        }

        private static float[][,] ConvertFromJaggedArray(float[][][] jagged)
        {
            var array = new float[jagged.Length][,];
            for (int i = 0; i < jagged.Length; i++)
            {
                int rows = jagged[i].Length;
                int cols = jagged[i][0].Length;
                array[i] = new float[rows, cols];
                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < cols; k++)
                    {
                        array[i][j, k] = jagged[i][j][k];
                    }
                }
            }
            return array;
        }
    }
}
