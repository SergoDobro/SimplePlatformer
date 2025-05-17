using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SDLibTemplate_v11.Game.MainGame;
using SDMonoLibUtilits;
using SDMonoLibUtilits.Scenes;
using SDMonoLibUtilits.Scenes.GUI;
using SDMonoLibUtilits.Utils;
using SDMonoUI.UI.Base;
using SDMonoUI.UI.Elements;
using Simple_Platformer.Game.MainGame.Other;

namespace SDLibTemplate_v11.Game.Menu
{
    public class MainMenu : ComplexScene
    {

        public override void Init()
        {

            RootScene.Instance.IsMouseVisible = false;

            RootScene.Instance.SetWindowSize(1224, 896);
            RootScene.Instance.SetWindowResolutionSize(700, 600);
            RootScene.Instance.mainBackground = Color.Tan;



            var slideshow = new Simple_Platformer.Game.MainGame.Other.ImageSlideshow(RootScene.Instance.GraphicsDevice)
            {
                Path = "Content\\Photos",
                TargetRectangle = RootScene.GetScreenResolution_rect
            };
            slideshow.NextImage();
            scenes.Add(slideshow);

            RootScene.controls.AddBinding(new SDMonoLibUtilits.KeyBindingsData(), "menu");

            RootScene.controls.keyBindingsData["menu"].SetContinuous(Keys.R, () =>
            {
                slideshow.NextImage();

            });


            scenes.Add(new GUI_Menu());
            base.Init();
        }
        public override void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            base.LoadContent(contentManager, graphicsDevice);
            scenes[0].Init();
        }
        public override void Update(float dt)
        {
            base.Update(dt);
        }
        public override void Draw(SpriteBatch _spriteBatch)
        {
            base.Draw(_spriteBatch);//ui is drawn here, TODO: Add UI here
        }
    }



    internal class GUI_Menu : AbstractGUI
    {
        float curDisplayedScore = 0;
        float targetDisplayedScore = 0;
        protected override void CreateUI()
        {
            DUIE_Outline.standartOutline = 2;
            DUIE_Outline.standartOutlineColor = new Color(Color.White, 0.8f);


            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = new Microsoft.Xna.Framework.Rectangle(200, 100, 300, 60),
                text = "Singleplayer",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.4f,
                mainColor = Color.Orange
            });
            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = new Microsoft.Xna.Framework.Rectangle(200, 210, 300, 60),
                text = "Multiplayer",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.4f,
                mainColor = Color.OrangeRed
            });
            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = new Microsoft.Xna.Framework.Rectangle(200, 320, 300, 60),
                text = "Options",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.4f,
                mainColor = Color.MonoGameOrange
            });
            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = new Microsoft.Xna.Framework.Rectangle(200, 430, 300, 60),
                text = "Quit",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.4f,
                mainColor = Color.Coral
            });


            new DataChanger((dc, totalTime, dt) =>
            {
                float easing = (float)Easings.EaseOutBounce(totalTime / 2f);
                (Elements[0] as Button).rectangle.Y = (int)(-(Elements[0] as Button).rectangle.Height * (1 - easing) + (easing) * (100));
                easing = (float)Easings.EaseOutBounce(totalTime / 2f);
                (Elements[1] as Button).rectangle.Y = (int)(-(Elements[0] as Button).rectangle.Height * (1 - easing) + (easing) * (210));
                easing = (float)Easings.EaseOutBounce(totalTime / 2f);
                (Elements[2] as Button).rectangle.Y = (int)(-(Elements[0] as Button).rectangle.Height * (1 - easing) + (easing) * (320));
                easing = (float)Easings.EaseOutBounce(totalTime / 2f);
                (Elements[3] as Button).rectangle.Y = (int)(-(Elements[0] as Button).rectangle.Height * (1 - easing) + (easing) * (430));
            }, (dc, totalTime) =>
            {

                for (int i = 0; i < Elements.Count(); i++)
                {
                    int index = i;
                    Rectangle initialRectangle1 = Elements[index].rectangle;
                    (Elements[index] as Button).HoverEnter += () => { DataChangers.IncreaseElement(0.2f, Elements[index], 5); };
                    (Elements[index] as Button).HoverExit += () => { DataChangers.DecreaseElement(0.2f, Elements[index], 5); };
                }

            }).AddAction_DeleteAfterTime(2f);


            SDMonoLibUtilits.Scenes.Particles.ParticleEffects.ParticleEffect_OnSquareArea partEff = new();

            RootScene.particle_system.CreateEffect(partEff
                );

            partEff.StartEffect(
                typeof(SDMonoLibUtilits.Scenes.Particles.ParticleTypes.Splash_RandomAppearOnScreen), new Rectangle(new Point(0, 0),
                RootScene.Instance.GetGraphicsScreenSize.ToPoint()),
                 duration: 10, particlesCount: 200,
                mainColor: new Color(Color.Yellow, 0.5f)
                );
            partEff.DeleteOnEnd = true;


            (Elements[0] as Button).LeftButtonPressed += () =>
            {
                RootScene.LoadScene(new GameScreen());
            };
            (Elements[1] as Button).LeftButtonPressed += () =>
            {
                RootScene.LoadScene(new GameScreen());
            };
            (Elements[3] as Button).LeftButtonPressed += RootScene.Instance.Exit;


            for (int i = 0; i < Elements.Count(); i++)
            {
                int index = i;
                (Elements[index] as ButtonEffected).AddEffectEarly(new DUIE_Outline(Elements[index]));
            }
            new DataChanger((dc, totalTime, dt) =>
            {
                float ease = (float)(Math.Pow(totalTime / 15f, 6));
                float ease2 = (float)(Math.Pow(totalTime / 15f, 2));
                for (int i = 0; i < Elements.Count; i++)
                {
                    ((Elements[i] as ButtonEffected).DUIEffectEarlyDraw as DUIE_Outline).outlineSize
                    = (int)Easings.Interpolate(ease2, 5, 2);
                    ((Elements[i] as ButtonEffected).DUIEffectEarlyDraw as DUIE_Outline).outlineColor = new Color(
                        Easings.Interpolate(ease, (float)Math.Sin(0 + 3 * totalTime + i / 2f), 255),
                        Easings.Interpolate(ease, (float)Math.Sin(5 + 3 * totalTime + i / 2f), 255),
                        Easings.Interpolate(ease, (float)Math.Sin(8 + 3 * totalTime + i / 2f), 255),
                        0.8f
                        );
                }
            }).AddAction_DeleteAfterTime(15f);



        }


    }
}
