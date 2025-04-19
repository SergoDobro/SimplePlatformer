using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameLogic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDLibTemplate_v11.Game.Menu;
using SDMonoLibUtilits;
using SDMonoLibUtilits.Scenes;
using SDMonoLibUtilits.Scenes.GUI;
using SDMonoUI.UI.Base.RectangleBuilder;
using SDMonoUI.UI.Elements;

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
        LevelData levelData;
        public override void Init()
        {
            RootScene.Instance.IsMouseVisible = false;


            SetBindings();

            camera = new Camera() { Zoom = 5};
            // Setup
            physics = new PhysicsManager();
            physics.SetCollision(CollisionGroup.Group1, CollisionGroup.Group2, true);

            levelData = new LevelDaata_Level1();
            levelData.LoadPremade();
            levelData.ApplyPhysics(physics);

            scenes.Add(new GameScreen_GUI());
            base.Init();
        }

        


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
            RootScene.Instance.mainBackround = Color.DarkBlue;
        }
        public override void Update(float dt)
        {
            base.Update(dt);
            if (!updateGame)
                return;


            camera.Follow(levelData.Player.RigidBody.Position);
            physics.Update(dt/3);
            physics.Update(dt/3);
            physics.Update(dt/3);
            levelData.Player.Update(dt);

        }

        public void SetBindings()
        {
            //bind your game events here

            RootScene.controls.AddBinding(new SDMonoLibUtilits.KeyBindingsData(), "game_controls");

            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.A, () => {
                levelData.Player.RigidBody.Velocity += new Vector2(-2, 0);

            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.D, () => {
                levelData.Player.RigidBody.Velocity += new Vector2(2, 0);
            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.W, () => {
                levelData.Player.ButtonUpPressed();
                
            });

            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.Q, () => {
                camera.Zoom *= 1.01f;
            });
            RootScene.controls.keyBindingsData["game_controls"].SetContinuous(Keys.E, () => {
                camera.Zoom *= 0.99f;
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

            foreach (var rb in TileGraphicsComponent.tileGraphicsComponents)
            {
                rb.Draw(_spriteBatch, textures,
                      cameraOffset: camera.Position,
                      scaleFactor: camera.Zoom,
                      color: Color.White,
                      sheetName: "simple_sheet");
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
            Position = (position + new Vector2(-10,-10)*Zoom)* p + (1- p) * Position;
        }
    }
}
