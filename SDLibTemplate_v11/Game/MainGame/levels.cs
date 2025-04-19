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
    public class Player : GameObject
    {
        static float JumpPower = 10;
        public void ButtonUpPressed()
        {

            if (kayoteTime>0)
            {
                RigidBody.Velocity += new Vector2(0, -JumpPower);
            }
        }
        bool prevSt = false;
        float kayoteTime = 0;
        public void Update(float dt)
        {
            if (RigidBody.IsCollidingDown)
            {
                kayoteTime = 0.15f;
            }
            else
            {
                kayoteTime -= dt * 1;
            }
            prevSt = RigidBody.IsCollidingDown;
        }
    }

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
    }
}