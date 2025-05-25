using ClassikNet;
using Microsoft.Xna.Framework;
using SDLibTemplate_v11.Game.MainGame;
using SDMonoLibUtilits;
using Simple_Platformer.Game.MainGame.GameObjectSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SDLib_Experiments.Game.MainGame
{
    public class Chamber
    {
        public float score;
        public float learnPower = 50;
        public float learnFrequence = 0.0001f;
        public Player player { get; set; }
        public ClassicNet classicNet;
        int successInRowCounter = 0;
        int totalFails = 0;
        float maxH = int.MinValue;
        float ticksTillMax = 0;
        Dictionary<string, float> data;
        int oldPos = 0;
        int oldPos_time = 0;
        public void Init()
        {
            learnPower = ChamberParameterLoader.LoadParametrs("learnPower");
            learnFrequence = ChamberParameterLoader.LoadParametrs("learnFrequence");
            totalFails = 0;
            score = 0;
            if (data is null)
                data = new Dictionary<string, float>();
            data.Clear();
            iter = 0;

        }
        public void Tick()
        {

            //gamecore.UpdateTick();
            var dat = GetGameData();
            if (dat.Count() == 0)
            {
                return;
            }
            var result = classicNet.RunNet(GetGameData());

            float out_left = result[0];
            float out_up = result[1];
            float out_right = result[2];

            if (out_left > out_right && out_left > 0.1) player.ButtonLeftPressed();
            if (out_right > out_left && out_right > 0.1) player.ButtonRightPressed();
            if (out_up > 0.1) player.ButtonUpPressed();

            score = -player.Position.Y / 100;
            if (-player.Position.Y > maxH)
            {
                maxH = -player.Position.Y;
            }

            memCells[0] = result[3];
            memCells[1] = result[4];
            memCells[2] = result[5];
        }
        public float prevX = 0;
        public float prevY = 0;
        public float iter = 0;
        public float[] memCells = new float[3];
        public Dictionary<string, float> GetGameData()
        {
            //
            data.Clear();

            data["velX"] = player.RigidBody.Velocity.X;
            data["velY"] = player.RigidBody.Velocity.Y;

            data["positionX"] = player.Position.X - prevX;
            data["positionY"] = player.Position.Y - prevY;
            Vector2 topPanel = ((RootScene.Instance.Root_scene as GameScreen).levelData as IAIPlatformerJumper).GetClosestPlatformBelowMyY(player.Position);
            data["distToTopX"] = (player.Position.X - topPanel.X) / 10f;
            data["distToTopY"] = (player.Position.Y - topPanel.Y) / 10f;

            topPanel = ((RootScene.Instance.Root_scene as GameScreen).levelData as IAIPlatformerJumper).GetClosestPlatformBeyondMyY(player.Position);
            data["distToBotX"] = (player.Position.X - topPanel.X) / 10f;
            data["distToBotY"] = (player.Position.Y - topPanel.Y) / 10f;


            topPanel = ((RootScene.Instance.Root_scene as GameScreen).levelData as IAIPlatformerJumper).GetClosestPlatformBeyondMyYX2(player.Position);
            data["distToTopTopX"] = (player.Position.X - topPanel.X) / 10f;
            data["distToTopTopY"] = (player.Position.Y - topPanel.Y) / 10f;

            data["CanJump"] = player.RigidBody.IsCollidingDown ? 1 : -1;
            data["memCell"] = memCells[0];
            data["memCell1"] = memCells[1];
            data["memCell2"] = memCells[2];
            //data["time"] = iter; 
            prevX = player.Position.X;
            prevY = player.Position.Y;
            //iter = Selector_platformer.TimeSinceLastSelection;

            return data;
        }
        public float[][] Tick_SnapshotNet()
        {
            var result = classicNet.RunNet_SnapshotNeurons(GetGameData());
            return result;
        }
        public float Evaluate()
        {
            return score / 400 + maxH - ((
                ((RootScene.Instance.Root_scene as GameScreen).levelData as IAIPlatformerJumper).GetClosestPlatformBelowMyY(player.Position) - player.Position).LengthSquared()

                //(((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBeyondMyY(player.Position) - player.Position).Length()
                ) / 20
                ;
        }
        public Chamber CloneChamber()
        {
            Chamber chamber = new Chamber();
            chamber.classicNet = classicNet.Clone();
            chamber.classicNet.MutateGenome_OneGenome(learnPower, learnFrequence);
            chamber.Init();
            return chamber;
        }
        public static Chamber CrossingOverChamber(ClassicNet classicNet)
        {
            Chamber chamber = new Chamber();
            chamber.classicNet = classicNet;
            chamber.Init();
            return chamber;
        }
    }

    public class ChamberParameterLoader
    {
        private const string FilePath = "ChamberParametrs.json";

        public static float LoadParametrs(string parameter)
        {
            Dictionary<string, float> parameters;

            // Load or create file
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                parameters = JsonSerializer.Deserialize<Dictionary<string, float>>(json) ?? new Dictionary<string, float>();
            }
            else
            {
                parameters = new Dictionary<string, float>();
                SaveParameters(parameters);
            }

            // Check if the parameter exists
            if (!parameters.ContainsKey(parameter))
            {
                parameters[parameter] = 0.0f; // Default value
                SaveParameters(parameters);
            }

            return parameters[parameter];
        }

        private static void SaveParameters(Dictionary<string, float> parameters)
        {
            string json = JsonSerializer.Serialize(parameters, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }

}
