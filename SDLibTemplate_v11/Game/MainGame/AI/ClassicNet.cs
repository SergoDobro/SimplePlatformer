using SDLibTemplate_v11.Game.MainGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClassikNet
{
    public class ClassicNet
    {
        public int[] layerInfo;
        public float[][,] layerNeuron_connections;
        public float[][] layerNeurons_biases;
        static public Dictionary<string, int> layerInputPosition = new Dictionary<string, int>()
        {
            { "positionX", 0},
            { "positionY", 1},
            { "distToTopX", 2},
            { "distToTopY", 3},
            { "distToBotX", 4},
            { "distToBotY", 5},
            { "distToTopTopX", 6},
            { "distToTopTopY", 7},
            { "velX", 8},
            { "velY", 9},
            { "memCell", 10},
            
        };

        static public Dictionary<int, string> layerOutputPosition_descriptor = new Dictionary<int, string>()
        {
            {  0, "out_left" },
            {  1, "out_up" },
            {  2, "out_right" },
            {  3, "memCell" },
            

        };
        public void Init(int[] layerSizes)
        {
            int layerCount = layerSizes.Length;
            layerInfo = new int[layerCount];
            layerNeuron_connections = new float[layerCount - 1][,]; //Last layer don't have such
            layerNeurons_biases = new float[layerCount][]; //each level have those

            for (int i = 0; i < layerInfo.Length; i++)
            {
                layerInfo[i] = layerSizes[i];
            }

            Randomize();

        }
        public void Randomize()
        {

            for (int i = 0; i < layerInfo.Length; i++)
            {
                layerNeurons_biases[i] = new float[layerInfo[i]];
                for (int j = 0; j < layerInfo[i]; j++)
                {
                    layerNeurons_biases[i][j] = (float)(Random.Shared.NextDouble() * 0.2f - 0.1f);
                }
            }
            for (int i = 0; i < layerInfo.Length - 1; i++)
            {
                layerNeuron_connections[i] = new float[layerInfo[i], layerInfo[i + 1]];
                for (int j = 0; j < layerInfo[i]; j++)
                {
                    for (int jj = 0; jj < layerInfo[i + 1]; jj++)
                    {
                        layerNeuron_connections[i][j, jj] = (float)(Random.Shared.NextDouble() * 0.2f - 0.1f);
                    }
                }
            }
        }

        public float[] RunNet(Dictionary<string, float> keyValuePairs)
        {
            return RunNet(layerInputPosition.OrderBy(x => x.Value).Select(x => Activate(keyValuePairs[x.Key])).ToArray());
        }

        public float[] RunNet(float[] firstLayer)
        {
            float[] layerFrom = firstLayer;
            for (int i = 0; i < layerInfo.Length - 1; i++)
            {
                layerFrom = CalculateLayer(i, layerFrom);
            }
            return layerFrom;
        }

        public float[][] RunNet_SnapshotNeurons(Dictionary<string, float> keyValuePairs)
        {
            return RunNet_SnapshotNeurons(layerInputPosition.OrderBy(x => x.Value).Select(x => Activate(keyValuePairs[x.Key])).ToArray());
        }
        public float[][] RunNet_SnapshotNeurons(float[] firstLayer)
        {
            float[][] layer_snapshots = new float[layerInfo.Length][];
            layer_snapshots[0] = firstLayer;

            float[] layerFrom = firstLayer;
            for (int i = 0; i < layerInfo.Length - 1; i++)
            {
                layerFrom = CalculateLayer(i, layerFrom);
                layer_snapshots[i + 1] = layerFrom;
            }
            return layer_snapshots;
        }

        public float[] CalculateLayer(int layer, float[] layerFrom)
        {
            float[] nextLayer = new float[layerInfo[layer + 1]];
            for (int i = 0; i < layerInfo[layer]; i++)
            {
                for (int j = 0; j < layerInfo[layer + 1]; j++)
                {
                    nextLayer[j] += layerFrom[i] * layerNeuron_connections[layer][i, j];
                }
            }

            for (int j = 0; j < layerInfo[layer + 1]; j++)
            {
                nextLayer[j] += layerNeurons_biases[layer + 1][j];
                nextLayer[j] = Activate(nextLayer[j]);
            }
            //now we got a layer
            return nextLayer;
        }

        public ClassicNet Clone()
        {

            ClassicNet classicNet = new ClassicNet();

            int layercount = layerInfo.Length;

            classicNet.Init(layerInfo.Clone() as int[]);


            for (int i = 0; i < layerInfo.Length; i++)
            {
                classicNet.layerInfo[i] = layerInfo[i];
                classicNet.layerNeurons_biases[i] = new float[layerInfo[i]];
                for (int j = 0; j < layerInfo[i]; j++)
                {
                    classicNet.layerNeurons_biases[i][j] = layerNeurons_biases[i][j];
                }
            }
            for (int i = 0; i < layerInfo.Length - 1; i++)
            {
                classicNet.layerNeuron_connections[i] = new float[layerInfo[i], layerInfo[i + 1]]; ;
                for (int j = 0; j < layerInfo[i]; j++)
                {
                    for (int jj = 0; jj < layerInfo[i + 1]; jj++)
                    {
                        classicNet.layerNeuron_connections[i][j, jj] = layerNeuron_connections[i][j, jj];
                    }
                }
            }
            return classicNet;
        }

        public void Mutate(float mutagenPower, float mutagenExpansion)
        {

            for (int i = 0; i < layerInfo.Length; i++)
            {
                for (int j = 0; j < layerInfo[i]; j++)
                {
                    if (Random.Shared.NextDouble() < mutagenExpansion)
                        layerNeurons_biases[i][j] += mutagenPower * (float)(Random.Shared.NextDouble() - 0.5);
                }
            }
            for (int i = 0; i < layerInfo.Length - 1; i++)
            {
                for (int j = 0; j < layerInfo[i]; j++)
                {
                    for (int jj = 0; jj < layerInfo[i + 1]; jj++)
                    {
                        if (Random.Shared.NextDouble() < mutagenExpansion)
                            layerNeuron_connections[i][j, jj] += mutagenPower * (float)(Random.Shared.NextDouble() - 0.5);
                    }
                }
            }
        }
        public void MutateGenome_OneLeayr(float mutagenPower, float mutagenExpansion)
        {

            int layerOfMutation = Random.Shared.Next(0, layerInfo.Length);

            for (int j = 0; j < layerInfo[layerOfMutation]; j++)
            {
                if (Random.Shared.NextDouble() < mutagenExpansion)
                    layerNeurons_biases[layerOfMutation][j] += mutagenPower * (float)(Random.Shared.NextDouble() - 0.5);
            }

            layerOfMutation = Random.Shared.Next(0, layerInfo.Length-1);

            for (int j = 0; j < layerInfo[layerOfMutation]; j++)
            {
                for (int jj = 0; jj < layerInfo[layerOfMutation + 1]; jj++)
                {
                    if (Random.Shared.NextDouble() < mutagenExpansion)
                        layerNeuron_connections[layerOfMutation][j, jj] += mutagenPower * (float)(Random.Shared.NextDouble() - 0.5);
                }
            }
        }

        public void MutateGenome_OneGenome(float mutagenPower, float mutagenExpansion)
        {

            int layerOfMutation = Random.Shared.Next(0, layerInfo.Length);
            int genOfMutation = Random.Shared.Next(0, layerInfo[layerOfMutation]);
            layerNeurons_biases[layerOfMutation][genOfMutation] += mutagenPower * (float)(Random.Shared.NextDouble() - 0.5);

            layerOfMutation = Random.Shared.Next(0, layerInfo.Length - 1);
            int genOfMutation_from = Random.Shared.Next(0, layerInfo[layerOfMutation]);

            for (int jj = 0; jj < layerInfo[layerOfMutation + 1]; jj++)
            {
                layerNeuron_connections[layerOfMutation][genOfMutation_from, jj] += mutagenPower * (float)(Random.Shared.NextDouble() - 0.5);
            }
        }


        public static ClassicNet Crossingover(ClassicNet parentA, ClassicNet parentB)
        {

            ClassicNet classicNet = new ClassicNet();

            int layercount = parentA.layerInfo.Length;

            classicNet.Init(parentA.layerInfo);


            for (int i = 0; i < parentA.layerInfo.Length; i++)
            {
                classicNet.layerInfo[i] = parentA.layerInfo[i];
                classicNet.layerNeurons_biases[i] = new float[parentA.layerInfo[i]];
                for (int j = 0; j < parentA.layerInfo[i]; j++)
                {
                    if (Random.Shared.NextDouble() < 0.5)
                        classicNet.layerNeurons_biases[i][j] = parentA.layerNeurons_biases[i][j];
                    else
                        classicNet.layerNeurons_biases[i][j] = parentB.layerNeurons_biases[i][j];
                }
            }
            for (int i = 0; i < parentA.layerInfo.Length - 1; i++)
            {
                classicNet.layerNeuron_connections[i] = new float[parentA.layerInfo[i], parentA.layerInfo[i + 1]]; ;
                for (int j = 0; j < parentA.layerInfo[i]; j++)
                {
                    for (int jj = 0; jj < parentA.layerInfo[i + 1]; jj++)
                    {
                        if (Random.Shared.NextDouble() < 0.5)
                            classicNet.layerNeuron_connections[i][j, jj] = parentA.layerNeuron_connections[i][j, jj];
                        else
                            classicNet.layerNeuron_connections[i][j, jj] = parentB.layerNeuron_connections[i][j, jj];
                    }
                }
            }
            return classicNet;
        }

        float _temp = 0;
        public float Activate(float number)
        {
            return number / ((float)Math.Sqrt(1 + number * number));
            return number < 0 ? 0 : number;
            _temp = (float)Math.Pow(2.71, -2 * number);
            return (float)((1 - _temp) / (1 + _temp));
        }
        public static float Activate_static(float number)
        {
            float _temp = (float)Math.Pow(2.71, -2 * number);
            return (float)((1 - _temp) / (1 + _temp));


        }


        
        public ClassicNet LoadInfo(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var options = GetJsonOptions();

                ClassicNet classicNet = new ClassicNet();
                classicNet.layerInfo = JsonSerializer.Deserialize<int[]>(json.Split("SPLITTER")[0], options);
                classicNet.layerNeurons_biases = JsonSerializer.Deserialize<float[][]>(json.Split("SPLITTER")[1], options);
                classicNet.layerNeuron_connections = JsonSerializer.Deserialize<float[][,]>(json.Split("SPLITTER")[2], options);


                Console.WriteLine($"{this.GetType().FullName} loaded! Watch out for lurking bugs.");
                return classicNet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Oops! Error loading {this.GetType().FullName}: {ex.Message}");
                // Pro tip: Never let the player see this message. They'll think we're amateurs!
            }
            return null;
        }

        // Save current state to JSON
        public void Save(string path)
        {
            try
            {
                var options = GetJsonOptions();
                string json_info = JsonSerializer.Serialize<int[]>(this.layerInfo, options);
                string json_biases = JsonSerializer.Serialize<float[][]>(this.layerNeurons_biases, options);
                string json_connections = JsonSerializer.Serialize<float[][,]>(this.layerNeuron_connections, options);
                File.WriteAllText(path,
                    json_info + "SPLITTER" +
                    json_biases + "SPLITTER" +
                    json_connections);
                Console.WriteLine($"{this.GetType().FullName} saved! Your future self will thank you.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Save failed! Reason: {ex.Message} (Did you anger the JSON gods?)");
            }
        }


        // Because we like pretty JSON and enums that make sense
        private JsonSerializerOptions GetJsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public string GetStructureString()
        {
            return string.Join('_', layerInfo);
        }
        public int[] GetParseString(string str)
        {
            return str.Split('_').Select(x => int.Parse(x)).ToArray();
        }
    }
}
