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
    public partial class ClassicNet
    {
        public int[] layerInfo;
        public float[][,] layerNeuron_connections;
        public float[][] layerNeurons_biases;
        public Dictionary<string, int> layerInputPosition;
        public Dictionary<int, string> layerOutputPosition_descriptor;

        public virtual bool UseFastDefiner { get; } = true;
        public virtual bool UseAutoDefineInOutLayerSizes { get; } = true;

        public virtual string[] DefineOutput_Fast()
        {
            return new string[]
                {
                    "out_left",
                    "out_up",
                    "out_right",
                    "memCell",
                };
        }
        public virtual Dictionary<int, string> DefineOutput_Full()
        {
            return new Dictionary<int, string>()
            {

                {  0, "out_left" },
                {  1, "out_up" },
                {  2, "out_right" },
                {  3, "memCell" },

            };
        }
        private Dictionary<int, string> DefineOutput()
        {
            if (!UseFastDefiner)
                return DefineOutput_Full();
            else
            {
                var arr = DefineOutput_Fast();
                var res = new Dictionary<int, string>();
                for (int i = 0; i < arr.Length; i++)
                {
                    res.Add(i, arr[i]);
                }
                return res;
            }
        }


        public virtual string[] DefineInput_Fast()
        {
            return new string[]
                {
                    "positionX",
                    "positionY",
                    "distToTopX",
                    "distToTopY",
                    "distToBotX",
                    "distToBotY",
                    "distToTopTopX",
                    "distToTopTopY",
                    "velX",
                    "velY",
                    "memCell",
                };
        }

        public virtual Dictionary<string, int> DefineInput_Full()
        {
            return new Dictionary<string, int>()
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
        }
        private Dictionary<string, int> DefineInput()
        {
            if (!UseFastDefiner)
                return DefineInput_Full();
            else
            {
                var arr = DefineInput_Fast();
                var res = new Dictionary<string, int>();
                for (int i = 0; i < arr.Length; i++)
                {
                    res.Add(arr[i], i);
                }
                return res;
            }
        }


        public void Init(int[] layerSizes, bool layers_provided_using_full_size = true)
        {

            layerOutputPosition_descriptor = DefineOutput();
            layerInputPosition = DefineInput();


            if (UseAutoDefineInOutLayerSizes && !layers_provided_using_full_size)
            {
                var _arr = new int[layerSizes.Length + 2];
                for (int i = 0; i < layerSizes.Length; i++)
                {
                    _arr[i + 1] = layerSizes[i];
                }

                layerSizes = _arr;
                layerSizes[0] = layerInputPosition.Count;
                layerSizes[layerSizes.Length - 1] = layerOutputPosition_descriptor.Count;
            }



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

        public float[] RunNet(float[] firstLayer)
        {
            float[] layerFrom = firstLayer;
            for (int i = 0; i < layerInfo.Length - 1; i++)
            {
                layerFrom = CalculateLayer(i, layerFrom);
            }
            return layerFrom;
        }
        /// <summary>
        /// Calculates the output of the next layer in a neural network using weighted connections and biases.
        /// </summary>
        /// <param name="layer">Index of the current layer.</param>
        /// <param name="inputs">Activations from the current layer.</param>
        /// <returns>Activated outputs of the next layer.</returns>
        public float[] CalculateLayer(int layer, float[] inputs)
        {
            int currentLayerSize = layerInfo[layer];
            int nextLayerSize = layerInfo[layer + 1];

            float[] outputs = new float[nextLayerSize];

            // Weighted sum of inputs
            for (int i = 0; i < currentLayerSize; i++)
            {
                for (int j = 0; j < nextLayerSize; j++)
                {
                    outputs[j] += inputs[i] * layerNeuron_connections[layer][i, j];
                }
            }

            // Add biases and apply activation
            for (int j = 0; j < nextLayerSize; j++)
            {
                outputs[j] += layerNeurons_biases[layer + 1][j];
                outputs[j] = Activate(outputs[j]);
            }

            return outputs;
             
        }


        public virtual ClassicNet GetCloningInstance()
        {
            return new ClassicNet();

        }
        public ClassicNet Clone()
        {

            ClassicNet classicNet = GetCloningInstance();

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

            layerOfMutation = Random.Shared.Next(0, layerInfo.Length - 1);

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

            ClassicNet classicNet = parentA.GetCloningInstance();

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
            //return number / ((float)Math.Sqrt(1 + number * number));
            return number < 0 ? 0 : number;
            _temp = (float)Math.Pow(2.71, -2 * number);
            return (float)((1 - _temp) / (1 + _temp));
        }
        public static float Activate_static(float number)
        {
            float _temp = (float)Math.Pow(2.71, -2 * number);
            return (float)((1 - _temp) / (1 + _temp));


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
