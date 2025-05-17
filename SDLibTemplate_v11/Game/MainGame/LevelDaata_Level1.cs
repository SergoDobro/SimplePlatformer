using GameLogic;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class LevelDaata_Level1 : LevelData
    {
        public override void LoadPremade()
        {

            spawnPoint = new Vector2(210, 50);
            // Create objects
            this.Player = new Player()
            {
                    Position = spawnPoint,
                
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
            for (int i = 0; i < 20; i++)
            {

                var platform = new GameObject()
                {
                    Position = new Vector2(200 + lastX, 100),
                };
                platform.AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group2,
                    IsKinematic = true,
                    Colliders = {
                            new Collider { Offset = Vector2.Zero, Size = new Vector2(10, 10) },
                            new Collider { Offset = new Vector2(10,-3.5f), Size = new Vector2(10, 3) } },
                });
                lastX += platform.RigidBody.Colliders[0].Size.X + 10;
                GameObjects["platforms"].TryAdd($"platform_{i}", platform);
            }

            lastX = 0;
            float lastY = 0;
            GameObjects.TryAdd("tiles", new Dictionary<string, GameObject>());
            for (int j = 0; j < 50; j++)
            {

                AddPlatform6(lastX + 50, 80 + lastY);
                lastX += /*GameObjects["tiles"].Last().Value.RigidBody.Colliders[0].Size.X*/ Random.Shared.Next(-50,51);
                lastY -= 19;
            }

            GenerateRandomUp();

            Save("Level_1");
        }
        List<Vector2> leftPlatformPositions = new List<Vector2>();
        public void AddPlatform6(float x, float y)
        {
            leftPlatformPositions.Add(new Vector2(x + 1 * 5, y));
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
                        Position = new Vector2(350 + Random.Shared.Next(-10,10), 10 - i*20),
                    
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
        public Vector2 GetClosestPlatformBelowMyY(Vector2 position)
        {

            var plat = leftPlatformPositions.First();
            for (int i = 0; i < leftPlatformPositions.Count; i++)
            {

                if (-leftPlatformPositions[i+1].Y > -position.Y)
                {
                    break;
                }
                else
                    plat = leftPlatformPositions[i];
            }
            return plat;

            //var plat = GameObjects["platforms"].First().Value;
            //for (int i = 0; i < GameObjects["platforms"].Count; i++)
            //{

            //    if (-GameObjects["platforms"][$"platform_{i}_up"].Position.Y > -position.Y)
            //    {
            //        break;
            //    }
            //    else
            //        plat = GameObjects["platforms"][$"platform_{i}_up"];
            //}
            //return plat.Position;
        }

        public Vector2 GetClosestPlatformBeyondMyY(Vector2 position)
        {

            var plat = leftPlatformPositions.First();
            for (int i = 0; i < leftPlatformPositions.Count-2; i++)
            {

                if (-leftPlatformPositions[i+2].Y > -position.Y)
                {
                    break;
                }
                else
                    plat = leftPlatformPositions[i];
            }
            return plat;

            //var plat = GameObjects["platforms"].First().Value;
            //for (int i = 0; i < GameObjects["platforms"].Count - 1; i++)
            //{

            //    if (-GameObjects["platforms"][$"platform_{i+1}_up"].Position.Y > -position.Y)
            //    {
            //        break;
            //    }
            //    else
            //        plat = GameObjects["platforms"][$"platform_{i}_up"];
            //}
            //return plat.Position;
        }

        internal Vector2 GetClosestPlatformBeyondMyYX2(Vector2 position)
        {
            var plat = leftPlatformPositions.First();
            for (int i = 0; i < leftPlatformPositions.Count - 3; i++)
            {

                if (-leftPlatformPositions[i+3].Y > -position.Y)
                {
                    break;
                }
                else
                    plat = leftPlatformPositions[i+3];
            }
            return plat;
        }
    }
}