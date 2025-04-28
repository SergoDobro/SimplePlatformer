using ClassikNet;
using GameLogic;
using Microsoft.Xna.Framework;
using SDLibTemplate_v11.Game.MainGame;
using SDMonoLibUtilits;
using Simple_Platformer.Game.MainGame.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SDLib_Experiments.Game.MainGame
{
    public class Selector
    {

        public List<Chamber> chamberList;
        public void Init(object data, int count = 350)
        {
            chamberList = new List<Chamber>();

            for (int i = 0; i < count; i++)
            {
                chamberList.Add(new Chamber());
                chamberList.Last().classicNet = new ClassicNet();
                chamberList.Last().classicNet.Init(new int[] { 11, 5, 5, 5, 4 });
                CreateChamberSpecificData(data, i, chamberList.Last());
            }
            foreach (var item in chamberList)
                item.Init();

        }
        public void CreateChamberSpecificData(object generaldata, int chamberId, Chamber chamber)
        {
            (generaldata as LevelData).GameObjects["AIs"].Add($"test_AI_{chamberId}",
                new Player()
                {
                    Position = new Vector2(140, 50),

                }.AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group1,
                    Colliders = { new Collider { Offset = Vector2.Zero, Size = new Vector2(2, 4) } },
                    Acceleration = new Vector2(0, 50 + 100)
                }).AddComponent(new AIComponent()
                {

                }).AddComponent(new GraphicsComponentExtended()
                {
                    blendPower = 0.5f
                }));
            chamber.player = (generaldata as LevelData).GameObjects["AIs"].Last().Value as Player;
        }
        public CancellationToken? cts;
        public void Tick()
        {

            Task[] ths = new Task[chamberList.Count];
            for (int i = 0; i < chamberList.Count; i++)
            {
                // Capture the current value of 'i' in a loop-local variable 'j'
                int j = i; // Fix: Create a local copy for each iteration
                if (cts is null)
                {

                    ths[j] = new Task(() =>
                    {
                        chamberList[j].Tick(); // Now 'j' is stable per task
                    });
                    ths[j].Start();
                }
                else
                {

                    ths[j] = new Task(() =>
                    {
                        chamberList[j].Tick(); // Now 'j' is stable per task
                    }, cts.Value);
                    if (!ths[j].IsCompleted)
                        ths[j].Start();
                }
            }
            foreach (var item in ths)
                item.Wait();
        }
        public virtual void Select()
        {
            var resList = chamberList.OrderByDescending(x => x.Evaluate()).ToList();
            BestInGenerationScore = resList[0].score;
            AverageInGenerationScore = resList.Sum(x => x.score) / (float)resList.Count;
            List<Player> freePlayers = new List<Player>(); //to rebind player on the map
            for (int i = (int)(resList.Count * 0.1f); i < resList.Count; i++)
            {
                if (5f * i / (float)resList.Count > Random.Shared.NextDouble())
                {
                    freePlayers.Add(resList[i].player);
                    resList.RemoveAt(i);
                    i--;
                }
            }
            int newBorns = chamberList.Count - resList.Count;
            if (newBorns > 0)
            {
                //resList.Add(resList[0].CloneChamber());
                //for (int i = 0; i < newBorns/2 && newBorns>0; i+=2)
                //{
                //    resList.Add(Chamber.CrossingOverChamber(ClassicNet.Crossingover(resList[0 + i].classicNet, resList[1 + i].classicNet)));
                //    newBorns--;
                //}
            }
            for (int i = 0; i < newBorns; i++)
            {
                if (i >= resList.Count)
                {
                    newBorns -= resList.Count;
                    i = 0;
                }
                resList.Add(resList[i].CloneChamber());
                resList.Last().player = freePlayers[i];

                //freePlayers.Add(resList[i].player);
            }
            chamberList.Clear();
            chamberList = resList;
            foreach (var item in chamberList)
            {
                item.Init();
            }

        }
        public float BestInGenerationScore;
        public float AverageInGenerationScore;
        public Chamber BestChamber()
        {
            return chamberList.OrderByDescending(x => x.Evaluate()).First();
        }

    }
    public class Selector_platformer : Selector
    {
        public static float TimeSinceLastSelection { get; set; }
        Vector2 oldVel;
        Vector2 oldPos;
        public void Reload()
        {

            Vector2 newStart = new Vector2(60 + Random.Shared.Next(-1, 1), 50);
            Vector2 newVelocity = new Vector2(Random.Shared.Next(-20, 20), -Random.Shared.Next(-20, 0)) * 0.5f;
            if (Random.Shared.NextDouble() < 0.3)
            {

                newVelocity = oldVel;
                newStart = oldPos;
            }

            foreach (var item in chamberList)
            {
                item.player.Position = newStart;
                item.player.RigidBody.Velocity = newVelocity;
                item.player.RigidBody.IsCollidingDown = false;
                item.memCell = 0;
            }
            oldVel = newVelocity;
            oldPos = newStart;
        }
        public override void Select()
        {
            Reload();
            var resList = chamberList.OrderByDescending(x => x.Evaluate()).ToList();
            BestInGenerationScore = resList[0].score;
            AverageInGenerationScore = resList.Sum(x => x.score) / (float)resList.Count;
            Queue<Player> freePlayers = new Queue<Player>();
            int c_orig = (int)(resList.Count);
            float portion = 0.05f;
            int c = (int)(resList.Count * portion);
            int c_k = (int)(resList.Count * (1 - portion));
            
            
            float DeathEnchancer = 5;


            for (int i = c; i < resList.Count; i++)
            {
                if (DeathEnchancer * (i - c) / (float)c_k > Random.Shared.NextDouble())
                {
                    freePlayers.Enqueue(resList[i].player);
                    resList.RemoveAt(i);
                    i--;
                }
            }
            int newBorns = chamberList.Count - resList.Count;
            int og = resList.Count;

            if (newBorns > 0)
            {

                for (int i = 0; i < newBorns / 2 && newBorns > 0 && i + 1 < og; i += 2)
                {
                    resList.Add(Chamber.CrossingOverChamber(ClassicNet.Crossingover(resList[0 + i].classicNet, resList[1 + i].classicNet)));
                    resList.Last().player = freePlayers.Dequeue();
                    newBorns--;


                    (resList.Last().player.GetComponent<GraphicsComponentExtended>() as GraphicsComponentExtended).main_color
                        = 
                        Color.FromNonPremultiplied((resList[0 + i].player.GetComponent<GraphicsComponentExtended>() as GraphicsComponentExtended).main_color.ToVector4() * 0.5f + 
                        (resList[1 + i].player.GetComponent<GraphicsComponentExtended>() as GraphicsComponentExtended).main_color.ToVector4() * 0.5f)

                        ;

                }
            }

            for (int i = 0; i < newBorns; i++)
            {
                if (i >= og)
                {
                    newBorns -= og;
                    i = 0;
                }
                resList.Add(resList[i].CloneChamber());
                resList.Last().player = freePlayers.Dequeue();
                (resList.Last().player.GetComponent<GraphicsComponentExtended>() as GraphicsComponentExtended).main_color
                    =
                    SDMonoLibUtilits.Utils.ColorHelper.Mix(
                        (resList[i].player.GetComponent<GraphicsComponentExtended>() as GraphicsComponentExtended).main_color,
                        SDMonoLibUtilits.Utils.ColorHelper.GetRandomColor(),
                        0.9f);
                ;

                //freePlayers.Add(resList[i].player);
            }
            chamberList.Clear();
            chamberList = resList;
            foreach (var item in chamberList)
            {
                item.Init();
            }

        }
    }
    public class Chamber
    {
        public float score;
        public Player player { get; set; }
        public ClassicNet classicNet;
        int successInRowCounter = 0;
        int totalFails = 0;
        float maxH = int.MinValue;
        float ticksTillMax = 0;
        public void Init()
        {
            totalFails = 0;
            ////if (gamecore is null)
            //    //gamecore = new Game();
            //gamecore.Init(true);
            //gamecore.OnRightWallHit += () => {
            //    //score =-totalFails;
            //    //totalFails++; ;
            //    //successInRowCounter = 0;
            //    //if (score>91)
            //    //{
            //    //    score -= 90;

            //    //} 
            //    score -= 10;
            //};
            //gamecore.OnReflectedByPlayer += () => { 
            //    score+=1;
            //    //score+=1 * successInRowCounter;
            //    successInRowCounter++;
            //    if (successInRowCounter >= 10)
            //    {
            //        successInRowCounter = 10;
            //    }
            //};
            score = 0;
            if (data is null)
                data = new Dictionary<string, float>();
            data.Clear();
            //data.Add("ballX", (gamecore.ballX - gamecore.mapWidth / 2) / (float)(gamecore.mapWidth / 2));
            //data.Add("ballY", (gamecore.ballY - gamecore.mapHeight / 2) / (float)(gamecore.mapHeight / 2));
            //data.Add("ballX_vel", gamecore.ballX_vel);
            //data.Add("ballY_vel", gamecore.ballY_vel);
            //data.Add("boardPosition", (gamecore.boardPosition - gamecore.mapHeight / 2) / (float)(gamecore.mapHeight / 2));


            //player.Position = new Vector2(140, 50);
            //prevX = player.Position.X;
            //prevY = player.Position.Y;
            iter = 0;

        }
        Dictionary<string, float> data;
        int oldPos = 0;
        int oldPos_time = 0;
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

            score /*+*/= -player.Position.Y/100;
            if (-player.Position.Y > maxH)
            {
                maxH = -player.Position.Y;
            }

            memCell = result[3];
        }
        public float prevX = 0;
        public float prevY = 0;
        public float iter = 0;
        public float memCell = 0;
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

            data["memCell"] = memCell;
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
            return score / 40 + maxH - ((
                ((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBelowMyY(player.Position) - player.Position).LengthSquared()

                //(((RootScene.Instance.Root_scene as GameScreen).levelData as LevelDaata_Level1).GetClosestPlatformBeyondMyY(player.Position) - player.Position).Length()
                ) / 20
                ;
        }
        public Chamber CloneChamber()
        {
            Chamber chamber = new Chamber();
            chamber.classicNet = classicNet.Clone();
            chamber.classicNet.MutateGenome_OneGenome(16, 0.09f);
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
