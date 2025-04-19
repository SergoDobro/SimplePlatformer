using GameLogic;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class LevelDaata_Level1 : LevelData
    {
        public override void LoadPremade()
        {

            // Create objects
            this.Player = new Player()
            {
                    Position = new Vector2(110, 50),
                
            };
            this.Player.AddComponent(new Rigidbody
            {
                Group = CollisionGroup.Group1,
                Colliders = { new Collider { Offset = Vector2.Zero, Size = new Vector2(2, 4) } },
                Acceleration = new Vector2(0, 50+100)
            });

            GameObjects.TryAdd("player", new Dictionary<string, GameObject>() {
                { "player_1", this.Player }
            });


            GameObjects.TryAdd("platforms", new Dictionary<string, GameObject>());
            float lastX = -100;
            for (int i = 0; i < 100; i++)
            {

                var platform = new GameObject()
                {
                    Position = new Vector2(100 + lastX, 100),
                };
                platform.AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group2,
                    IsKinematic = true,
                    Colliders = {
                            new Collider { Offset = Vector2.Zero, Size = new Vector2(10, 10) },
                            new Collider { Offset = new Vector2(10,0), Size = new Vector2(10, 3) } },
                });
                lastX += platform.RigidBody.Colliders[0].Size.X + 10;
                GameObjects["platforms"].TryAdd($"platform_{i}", platform);
            }

            lastX = 0;
            float lastY = 0;
            GameObjects.TryAdd("tiles", new Dictionary<string, GameObject>());
            for (int j = 0; j < 50; j++)
            {

                addPlatform(lastX + 100, 80 + lastY);
                lastX += /*GameObjects["tiles"].Last().Value.RigidBody.Colliders[0].Size.X*/ Random.Shared.Next(-20,21);
                lastY -= 16;
            }

            GenerateRandomUp();

            Save("Level_1");
        }
        public void addPlatform(float x, float y)
        {
            for (int i = 0; i < 3; i++)
            {

                var platform = new GameObject()
                {
                    Position = new Vector2(x + i*5, y),
                };
                platform.AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group2,
                    IsKinematic = true,
                    Colliders = {
                            new Collider { Offset = Vector2.Zero, Size = new Vector2(5, 5) }
                    }
                });
                platform.AddComponent(new TileGraphicsComponent() { tilesheetPosId = (i == 0 ? (0) : (i == 2 ? 2 : 1)) });

                GameObjects["tiles"].TryAdd($"tile_{x}_{y}_{i}", platform);



                var platform2 = new GameObject()
                {
                    Position = new Vector2(x + i * 5, y + 5),
                };
                platform2.AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group2,
                    IsKinematic = true,
                    Colliders = {
                            new Collider { Offset = Vector2.Zero, Size = new Vector2(5, 5) }
                    }
                });
                platform2.AddComponent(new TileGraphicsComponent() { tilesheetPosId = (i == 0 ? (6) : (i == 2 ? 8 : 7)) });

                GameObjects["tiles"].TryAdd($"tile_{x}_{y}_{i}_bot", platform2);
            }
        }
        private void GenerateRandomUp()
        {
            float lastX = 0;
            for (int i = 0; i < 100; i++)
            {


                var platform = new GameObject()
                {
                        Position = new Vector2(100 + Random.Shared.Next(-10,10), 100 - i*20),
                    
                };
                platform.AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group2,
                    IsKinematic = true,
                    Colliders = {
                            new Collider { Offset = Vector2.Zero, Size = new Vector2(10, 2) }
                        },
                });
                lastX += platform.RigidBody.Colliders[0].Size.X + 10;
                GameObjects["platforms"].TryAdd($"platform_{i}_up", platform);
            }
        }
    }
}