using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClassikNet;
using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDLib_Experiments.Game.MainGame;
using SDLibTemplate_v11.Game.Menu;
using SDMonoLibUtilits;
using SDMonoLibUtilits.Scenes;
using SDMonoLibUtilits.Scenes.Frames;
using SDMonoLibUtilits.Scenes.GUI;
using SDMonoUI.UI.Base.RectangleBuilder;
using SDMonoUI.UI.Elements;
using Simple_Platformer.Game.MainGame.Components;
using Simple_Platformer.Game.MainGame.GameObjectSystem;
using Simple_Platformer.Game.MainGame.Other;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class GameScreen : ComplexScene
    {
        /// <summary>
        /// replace this object to your gamecore file with all session data
        /// </summary>
        public object gamecore;

        Dictionary<string, Texture2D> textures;
        PhysicsManager physics;
        Camera camera;
        public LevelData levelData;


        public ClassicNet classicNet;
        Selector selector;
        float[][] lastNetSnapshot;
        CancellationTokenSource cts = new CancellationTokenSource();
        Effect effect;

        ChartFrame chartFrame;
        public override void Init()
        {
            RootScene.Instance.IsMouseVisible = false;


            SetBindings();

            camera = new Camera() { Zoom = 5 };
            // Setup
            physics = new PhysicsManager();
            physics.SetCollision(CollisionGroup.Group1, CollisionGroup.Group2, true);
            physics.SetCollision(CollisionGroup.Group1, CollisionGroup.Group3, true);
            physics.SetCollision(CollisionGroup.Group2, CollisionGroup.Group3, true);

            levelData = new LevelDaata_Level1();
            levelData.LoadPremade();

            scenes.Add(new GameScreen_GUI());

            chartFrame = new SDMonoLibUtilits.Scenes.Frames.ChartFrame()
            {
                chartData = new SDMonoLibUtilits.Scenes.Frames.ChartData()
                {
                    dataPoints = new Dictionary<int, double[]>()
                    {
                    }
                },
                rectangle_for_screen = RB_rel.GetRect(
                    0, 1f, 0.6f, 1f,
                0.1f, 0.1f, 0.05f, 0.05f,
                RootScene.GetScreenResolution_rect)

            };
            scenes.Add(chartFrame);


            levelData.GameObjects.Add("AIs", new Dictionary<string, GameObject>());

            LoadAI();
            LoadMusic();




            base.Init();

            RootScene.Instance.Exiting += (arg1, arg2) => { cts.Cancel(); };
            (scenes[0] as GameScreen_GUI).SubscribeOnExit(cts.Cancel);

        }

        private void LoadMusic()
        {
            Mp3Player mp3Player = new Mp3Player("Content\\Audio\\NonMGCBBuild\\Summer-Sport(chosic.com).mp3");
            mp3Player.Start();
            mp3Player.BeatSensitivity = 1.4f;
            mp3Player.EnergyDecayRate = 0.95f;
            mp3Player.BeatDetected += (mp3_player, args) =>
            {
                beatDelta += 2;
            };
        }

        private void LoadAI()
        {
            selector = new Selector_platformer();
            string path = "data.txt";
            int num = 100;
            int[] parameters = new int[] { 11, 10, 5, 5, 4 };
            if (!File.Exists(path))
            {
                File.Create(path).Close();
                File.WriteAllLines(path, new string[] {
                "300",
                "11, 20, 10, 5, 4"
                });
            }
            else
            {
                try
                {
                    var dat = File.ReadAllLines(path);
                    num = int.Parse(dat[0]);
                    parameters = dat[1].Split(", ").Select(x => int.Parse(x)).ToArray();
                }
                catch
                {

                }
            }

            selector.Init(levelData, num, parameters);
            //gamecore = selector.chamberList.OrderBy(x => x.score).First().gamecore;//new Game(); 
            classicNet = selector.chamberList.OrderBy(x => x.score).First().classicNet;

            //classicNet.Init(3, new int[] { 6, 3, 3 });
            selector.cts = cts.Token;
            var mainTask = new Task(() =>
            {
                //return;
                float extraTime = 0;
                float maxExtra = 0;
                for (int i = 0; i < 1000; i++)
                {
                    safer = true;
                    Thread.Sleep((100 + (int)(Math.Log((i + 2) / 2f, 1.5) * 2000 + maxExtra * 1500)) / 10);
                    safer = false;
                    Thread.Sleep(100);
                    extraTime = (selector.chamberList.OrderBy(x => -x.player.Position.Y).First().player.Position.Y) / 100;
                    if (extraTime < 0)
                        extraTime = 0;
                    if (extraTime > maxExtra)
                        maxExtra = extraTime;
                    chartFrame.chartData.dataPoints.Add(i + 1, selector.chamberList.Select(x =>
                    {
                        double _data = (double)x.Evaluate();
                        if (_data < -100)
                            _data = -100;
                        return _data;
                    }).ToArray());
                    chartFrame.RefreshData();
                    selector.Select();
                    selectionStartedTime = totalTime;

                    for (int j = 0; j < 50 + 15 * (i - i % 3) /*+ maxExtra * 64*/; j++)
                    {

                        float dt = 1 / 120f;
                        physics.Update(dt);
                        levelData.Player.Update(dt);

                        //levelData.Player.ButtonUpPressed();


                        selector.Tick();

                        foreach (var item in levelData.GameObjects["AIs"])
                        {
                            (item.Value as Player).Update(dt);
                        }
                    }

                    Thread.Sleep(100);
                }
                return;
                Thread.Sleep(1000);
                for (int i = 0; i < 1; i++)
                {
                    Chamber _chamber = selector.BestChamber();
                    int k = 15;
                    int gameLength = 4000;


                    _chamber = selector.BestChamber();
                    //gamecore = _chamber.gamecore;//new Game(); 
                    classicNet = _chamber.classicNet;
                    SetBindings();

                    Thread.Sleep(200);

                    for (int j = 0; j < 3000; j++)
                    {
                        Thread.Sleep(4);
                        _chamber.Tick();
                        lastNetSnapshot = _chamber.Tick_SnapshotNet();
                        //(scenes[0] as GameScreen_GUI).label_score.text = $"Score: {_chamber.score}";
                    }
                    _chamber.score = 0;
                    Thread.Sleep(500);
                }
            }, cts.Token);
            mainTask.Start();
        }

        /// <summary>
        /// can freely use ais
        /// </summary>
        bool safer = false;

        float beatDelta = 0;


        public override void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {

            base.LoadContent(contentManager, graphicsDevice);
            scenes[0].Init();


            textures = new Dictionary<string, Texture2D>
            {
                { "none", new Texture2D(graphicsDevice, 1, 1) }
            };
            textures["none"].SetData(new Color[] { Color.White });
            textures.Add("simple_sheet", contentManager.Load<Texture2D>("Textures\\simple_sheet"));
            RootScene.Instance.mainBackground = Color.DarkBlue;
            effect = contentManager.Load<Effect>("Shaders\\CoolPlatform");

        }
        float selectionStartedTime = 0;
        float totalTime = 0;
        public int frame = 0;
        public override void Update(float dt)
        {
            base.Update(dt);
            if (!updateGame)
                return;
            beatDelta *= 0.9f;
            totalTime += dt;
            frame++;
            //if (frame % 8 != 0)
            //    return;


            Selector_platformer.TimeSinceLastSelection = totalTime - selectionStartedTime;

            camera.Follow(levelData.Player.RigidBody.Position);
            //physics.Update(dt / 2);
            //physics.Update(dt/3);
            //physics.Update(dt/3);
            //AIComponent.SimulateAIs();
            if (safer)
            {
                physics.Update(dt / 2);
                levelData.Player.Update(dt / 2);
                foreach (var item in levelData.GameObjects["AIs"])
                {
                    (item.Value as Player).Update(dt / 2);
                }
                selector.Tick();

            }

            try
            {

                foreach (var item in GameObjectUpdater.gameObjectUpdater.Keys)
                {
                    GameObjectUpdater.gameObjectUpdater[item].Update(dt);
                }

            }
            catch
            {

            }
        }

        public void SetBindings()
        {
            //bind your game events here

            RootScene.controls.AddBinding(new SDMonoLibUtilits.KeyBindingsData(), "game_controls");

            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.A, () =>
            {
                levelData.Player.ButtonLeftPressed();

            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.D, () =>
            {
                levelData.Player.ButtonRightPressed();
            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.W, () =>
            {
                levelData.Player.ButtonUpPressed();

            });

            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.LeftControl, () =>
            {
                levelData.Player.ButtonPush();

            });


            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.LeftControl, () =>
            {
                //cts.Cancel();

            });



            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.Q, () =>
            {

                camera.Zoom *= 1.01f;
            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.E, () =>
            {
                camera.Zoom *= 0.99f;
            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.Space, () =>
            {
                levelData.Player.Position = new Vector2(levelData.Player.Position.X, -1000);
            });

            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.T, () =>
            {
                var bestClassicNet = selector.BestChamber().classicNet;
                bestClassicNet.Save($"ClassicNet_{classicNet.GetStructureString()}");

            });

            RootScene.controls.keyBindingsData["game_controls"].SetClick(Keys.Enter, () =>
            {
                
                levelData.Player.LaunchShot();
            });



        }




        public override void Draw(SpriteBatch _spriteBatch)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            foreach (var rb in physics._rigidbodies)
            {
                rb.Draw(_spriteBatch, textures,
                      cameraOffset: camera.Position,
                      scaleFactor: camera.Zoom,
                      color: Color.White);
            }
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred,
                effect: effect);
            foreach (var rb in TileGraphicsComponent.tileGraphicsComponents)
            {
                rb.Draw(_spriteBatch, textures,
                      cameraOffset: camera.Position,
                      scaleFactor: camera.Zoom,
                      color: new Color(
                          (float)Math.Sin(DateTime.Now.Second * 0.1 + beatDelta),
                      (float)Math.Sin(DateTime.Now.Second * 0.14 + 1 + beatDelta),
                      (float)Math.Sin(DateTime.Now.Second * 0.11 + 3 + beatDelta)) * 0.5f,
                      sheetName: "simple_sheet");
            }
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            foreach (var rb in GraphicsComponentExtended.GraphicsExtendedComponents)
            {
                rb.Draw(_spriteBatch, textures,
                      cameraOffset: camera.Position,
                      scaleFactor: camera.Zoom,
                      color: new Color(
                          (float)Math.Sin(DateTime.Now.Second * 0.1 + beatDelta),
                      (float)Math.Sin(DateTime.Now.Second * 0.14 + 1 + beatDelta),
                      (float)Math.Sin(DateTime.Now.Second * 0.11 + 3 + beatDelta)) * 0.5f
                      );
            }


            _spriteBatch.End();
            base.Draw(_spriteBatch);
        }

        #region pause

        bool updateGame = true;
        public void PauseClick()
        {
            (scenes[0] as GameScreen_GUI).StartAnimationOpenPause();

        }
        public void ReturnToGame()
        {
            (scenes[0] as GameScreen_GUI).StartAnimationClosePause();
        }

        #endregion


    }

    // Camera class (simplified)
    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 1.0f;
        public void Follow(Vector2 position)
        {
            float p = 0.9f;
            Position = (position - RootScene.GetGraphicsSize_v / (Zoom * (4))) * p + (1 - p) * Position;
        }
    }
}
