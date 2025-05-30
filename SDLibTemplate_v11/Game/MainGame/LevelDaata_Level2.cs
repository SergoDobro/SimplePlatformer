using GameLogic;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Simple_Platformer.Game.MainGame.GameObjectSystem;
using Simple_Platformer.Game.MainGame.Components;
using SDMonoUI.UI.Elements;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class LevelDaata_Level2 : LevelData, IAIPlatformerJumper
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
                Acceleration = new Vector2(0, 50 + 100)
            });

            GameObjects.TryAdd("player", new Dictionary<string, GameObject>() {
                { "player_1", this.Player }
            });


            
            GameObjects.TryAdd("platforms", new Dictionary<string, GameObject>());
            GameObjects.TryAdd("Squares", new Dictionary<string, GameObject>());
            GameObjects.TryAdd("flags", new Dictionary<string, GameObject>());
            float lastX = -100;
            for (int i = 0; i < 20; i++)
            {

                var platform = new GameObject()
                {
                    Position = new Vector2(lastX, 100),
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


            Generate_BigPlatforms_Up();
            Save("Level_2");
        }
        public void Generate_BigPlatforms_Up()
        {


            float lastX = 0;
            float lastY = 0;
            Random random = new Random(42);
            GameObjects.TryAdd("tiles", new Dictionary<string, GameObject>());
            for (int j = 0; j < 50; j++)
            {

                AddPlatform6(lastX + 50, 80 + lastY);
                lastX += (float)(random.Next(-50, 51)*(1.2+Math.Sin(lastY/100)));
                lastY -= 19;
            }
        }
        List<Vector2> leftPlatformPositions = new List<Vector2>();
        public void AddPlatform6(float x, float y)
        {
            leftPlatformPositions.Add(new Vector2(x + 1 * 5, y));
            for (int i = 0; i < 3; i++)
            {

                var platform = new GameObject()
                {
                    Position = new Vector2(x + i * 5, y),
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

            if (leftPlatformPositions.Count() % 5 != 0)
                return;
            y -= 7.5f;
            x += 3;
            Flag flag = new Flag() { Position = new Vector2(x,y)};
            GameObjects["flags"].TryAdd($"flag_{x}_{y}_", flag);

            x += 100;
            for (int i = 0; i < 1; i++)
            {
                var Square = new GameObject();
                SafeAddGameObject(Square, $"flag_{x}_{y}_", "measurements")
                    .AddComponent(new Texture2DComponent()
                {
                    textureName = "square",
                    rectangle = new RectangleF(10, 10)
                }).AddComponent(new Rigidbody
                {
                    Group = CollisionGroup.Group2,
                    IsKinematic = true,
                    Colliders = {
                            new Collider { Offset = Vector2.Zero, Size = new Vector2(10, 10) }
                    }
                }); ; 
                Square.Position = new Vector2(x + 20*i,y);

            }
        }
        public Vector2 GetClosestPlatformBelowMyY(Vector2 position)
        {

            var plat = leftPlatformPositions.First();
            for (int i = 0; i < leftPlatformPositions.Count; i++)
            {

                if (-leftPlatformPositions[i + 1].Y > -position.Y)
                {
                    break;
                }
                else
                    plat = leftPlatformPositions[i];
            }
            return plat;
             
        }

        public Vector2 GetClosestPlatformBeyondMyY(Vector2 position)
        {

            var plat = leftPlatformPositions.First();
            for (int i = 0; i < leftPlatformPositions.Count - 2; i++)
            {

                if (-leftPlatformPositions[i + 2].Y > -position.Y)
                {
                    break;
                }
                else
                    plat = leftPlatformPositions[i];
            }
            return plat; 
        }

        public Vector2 GetClosestPlatformBeyondMyYX2(Vector2 position)
        {
            var plat = leftPlatformPositions.First();
            for (int i = 0; i < leftPlatformPositions.Count - 3; i++)
            {

                if (-leftPlatformPositions[i + 3].Y > -position.Y)
                {
                    break;
                }
                else
                    plat = leftPlatformPositions[i + 3];
            }
            return plat;
        }
    }
}