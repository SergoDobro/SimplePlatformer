using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace SDLibTemplate_v11.Game.MainGame
{

    public class LevelData
    {
        // Key: Category (e.g., "Enemies"), Key: Object ID, Value: GameObject
        public Dictionary<string, Dictionary<string, GameObject>> GameObjects { get; set; } = new Dictionary<string, Dictionary<string, GameObject>>();

        // The star of the show!
        public Player Player { get; set; } = new ();

        // Magic happens here: Load JSON into this object
        public void LoadInfo(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                var options = GetJsonOptions();
                LevelData loadedData = JsonSerializer.Deserialize<LevelData>(json, options)!;

                GameObjects = loadedData.GameObjects;
                Player = loadedData.Player;

                Console.WriteLine("Level loaded! Watch out for lurking bugs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Oops! Error loading level: {ex.Message}");
                // Pro tip: Never let the player see this message. They'll think we're amateurs!
            }
        }

        // Flatten all objects into an array (including player)
        public GameObject[] GetGameObjects()
        {
            List<GameObject> allObjects = new List<GameObject>();

            foreach (var category in GameObjects.Values)
            {
                allObjects.AddRange(category.Values);
            }

            if (Player != null)
            {
                allObjects.Add(Player);
            }

            Console.WriteLine($"Loaded {allObjects.Count} objects. Let the chaos begin!");
            return allObjects.ToArray();
        }

        // Save current state to JSON
        public void Save(string path)
        {
            try
            {
                var options = GetJsonOptions();
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(path, json);
                Console.WriteLine("Game saved! Your future self will thank you.");
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

        // TODO: Uncomment this if we add GameObject.Category and Id properties later
        // public void OrganizeObjects(GameObject[] objects)
        // {
        //     GameObjects.Clear();
        //     foreach (var go in objects)
        //     {
        //         if (!GameObjects.ContainsKey(go.Category))
        //             GameObjects[go.Category] = new Dictionary<string, GameObject>();
        //         GameObjects[go.Category][go.Id] = go;
        //     }
        // }
        public virtual void LoadPremade()
        {

        }
        public void ApplyPhysics(PhysicsManager physicsManager)
        {
            foreach (var item in GetGameObjects())
            {
                physicsManager.AddRigidbody(item.RigidBody);
            }
        }
    }
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