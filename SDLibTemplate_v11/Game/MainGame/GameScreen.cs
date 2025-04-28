using System;
using System.Collections.Generic;
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

            levelData = new LevelDaata_Level1();
            levelData.LoadPremade();

            scenes.Add(new GameScreen_GUI());

            chartFrame =
            (new SDMonoLibUtilits.Scenes.Frames.ChartFrame()
            {
                chartData = new SDMonoLibUtilits.Scenes.Frames.ChartData()
                {
                    dataPoints = new Dictionary<int, double[]>()
            {
                { 0,new double[500]}
            }
                },
                rectangle_for_screen = RB_rel.GetRect(
                    0, 1f, 0.6f, 1f,
                0.1f, 0.1f, 0.05f, 0.05f,
                RootScene.GetScreenResolution_rect)

            });
            scenes.Add(chartFrame);
            base.Init();


            levelData.GameObjects.Add("AIs", new Dictionary<string, GameObject>());
            //levelData.GameObjects["AIs"].Add("test_A",
            //new Player()
            //{
            //    Position = new Vector2(110, 50),

            //}.AddComponent(new Rigidbody
            //{
            //    Group = CollisionGroup.Group1,
            //    Colliders = { new Collider { Offset = Vector2.Zero, Size = new Vector2(2, 4) } },
            //    Acceleration = new Vector2(0, 50 + 100)
            //}).AddComponent(new AIComponent()
            //{

            //}));



            selector = new Selector_platformer();
            selector.Init(levelData);
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
                        if (_data < -500)
                            _data = -500;
                        return _data;
                    }).ToArray());
                    chartFrame.RefreshData();
                    selector.Select();
                    selectionStartedTime = totalTime;

                    for (int j = 0; j < -50 + 15 * i /*+ maxExtra * 64*/; j++)
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
                    for (int j = 1; j <= gameLength * k; j++)
                    {
                        selector.Tick();
                        if (j % (50) == 0)
                        {
                            //(scenes[0] as GameScreen_GUI).label_iteration.text = $"Iteration: {Math.Ceiling(100 * j / ((float)gameLength * k))}%";
                            //(scenes[0] as GameScreen_GUI).label_score.text = $"Score: {_chamber.score}";
                        }
                        if (j % (gameLength) == 0)
                        {
                            //selector.Select();
                            //(scenes[0] as GameScreen_GUI).label_average_gen_score.text = $"Average Generation Score: {selector.AverageInGenerationScore}";
                            //(scenes[0] as GameScreen_GUI).label_top_gen_score.text = $"Top Generation Score: {selector.BestInGenerationScore}";


                            _chamber = selector.BestChamber();
                            //gamecore = _chamber.gamecore;//new Game(); 
                            classicNet = _chamber.classicNet;
                        }
                    }

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

            RootScene.Instance.Exiting += (arg1, arg2) => { cts.Cancel(); };
            (scenes[0] as GameScreen_GUI).SubscribeOnExit(cts.Cancel);

        }
        /// <summary>
        /// can freely use ais
        /// </summary>
        bool safer = false;



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

                /*
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
                })

                 */

                //levelData.Player.Position = new Vector2(levelData.Player.Position.X, -1000);
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
                          (float)Math.Sin(DateTime.Now.Second * 0.1),
                      (float)Math.Sin(DateTime.Now.Second * 0.14 + 1),
                      (float)Math.Sin(DateTime.Now.Second * 0.11 + 3)) * 0.5f,
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
                          (float)Math.Sin(DateTime.Now.Second * 0.1),
                      (float)Math.Sin(DateTime.Now.Second * 0.14 + 1),
                      (float)Math.Sin(DateTime.Now.Second * 0.11 + 3)) * 0.5f
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
