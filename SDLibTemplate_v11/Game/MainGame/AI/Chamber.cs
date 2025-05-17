using ClassikNet;
using Microsoft.Xna.Framework;
using SDLibTemplate_v11.Game.MainGame;
using SDMonoLibUtilits;
using Simple_Platformer.Game.MainGame.GameObjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDLib_Experiments.Game.MainGame
{
    public class Chamber
    {
        public float score;
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
            Vector2 topPanel = ((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBelowMyY(player.Position);
            data["distToTopX"] = (player.Position.X - topPanel.X) / 10f;
            data["distToTopY"] = (player.Position.Y - topPanel.Y) / 10f;

            topPanel = ((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBeyondMyY(player.Position);
            data["distToBotX"] = (player.Position.X - topPanel.X) / 10f;
            data["distToBotY"] = (player.Position.Y - topPanel.Y) / 10f;


            topPanel = ((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBeyondMyYX2(player.Position);
            data["distToTopTopX"] = (player.Position.X - topPanel.X) / 10f;
            data["distToTopTopY"] = (player.Position.Y - topPanel.Y) / 10f;

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
                ((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBelowMyY(player.Position) - player.Position).LengthSquared()

                //(((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBeyondMyY(player.Position) - player.Position).Length()
                ) / 200
                ;
        }
        public Chamber CloneChamber()
        {
            Chamber chamber = new Chamber();
            chamber.classicNet = classicNet.Clone();
            chamber.classicNet.MutateGenome_OneGenome(50, 0.0001f);
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
}
