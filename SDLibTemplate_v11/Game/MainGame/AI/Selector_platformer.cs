using ClassikNet;
using GameLogic;
using Microsoft.Xna.Framework;
using SDLibTemplate_v11.Game.MainGame;
using Simple_Platformer.Game.MainGame.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SDLib_Experiments.Game.MainGame
{
    public class Selector_platformer : Selector
    {
        public override void CreateChamberSpecificData(object generaldata, int chamberId, Chamber chamber)
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
        public static float TimeSinceLastSelection { get; set; }
        Vector2 oldVel;
        Vector2 oldPos;
        public void Reload()
        {

            Vector2 newStart = new Vector2(60 + Random.Shared.Next(-1, 1), 50);
            Vector2 newVelocity = new Vector2(Random.Shared.Next(-20, 20), -Random.Shared.Next(-20, 0)) * 0.1f;
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
                for (int i = 0; i < item.memCells.Length; i++)
                {
                    item.memCells[i] = 0;
                } 
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

            UpdateParametrs(resList);            

            for (int i = number_of_unkillable; i < resList.Count; i++)
            {
                if (deathEnchancer * (i - number_of_unkillable) / (float)number_of_killed > Random.Shared.NextDouble())
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
}
