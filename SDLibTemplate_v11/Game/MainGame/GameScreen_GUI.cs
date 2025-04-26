using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using SDLibTemplate_v11.Game.Menu;
using SDMonoLibUtilits;
using SDMonoLibUtilits.Scenes.GUI;
using SDMonoLibUtilits.Utils;
using SDMonoUI.UI.Base;
using SDMonoUI.UI.Base.RectangleBuilder;
using SDMonoUI.UI.Elements;

namespace SDLibTemplate_v11.Game.MainGame
{
    internal class GameScreen_GUI : AbstractGUI
    {
        protected override void CreateUI()
        {
            #region start settings and effects 



            SDMonoLibUtilits.Scenes.Particles.ParticleEffects.ParticleEffect_OnSquareArea partEff = new();
            RootScene.particle_system.CreateEffect(partEff);

            partEff.StartEffect(
                typeof(SDMonoLibUtilits.Scenes.Particles.ParticleTypes.Splash_RandomAppearOnScreen), new Rectangle(new Point(0, 0),
                RootScene.Instance.GetGraphicsScreenSize.ToPoint()),
                 duration: 10, particlesCount: 200,
                mainColor: new Color(Color.Yellow, 0.5f)
            );
            partEff.DeleteOnEnd = true;
            #endregion
            CreatePauseGUI();
            CreateGameGUI();

        }

        List<DrawableUIElement> GameScreenElements = new List<DrawableUIElement>();
        private void CreateGameGUI()
        {


            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = RB.GetRect(700 - 100, 700, 0, 50, 3, 3, 3, 3),
                text = "Pause",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.3f,
                mainColor = Color.MonoGameOrange
            });
            (Elements.Last() as ButtonEffected).AddEffectEarly(new DUIE_Outline(Elements.Last()));
            GameScreenElements.Add(Elements.Last());
            pause_button_baseY_temp = Elements.Last().rectangle.Y;



            int initialY = GameScreenElements[0].rectangle.Y;

            new DataChanger((s, totalTime, dt) =>
            {
                float easing = (float)Easings.EaseOutBack(totalTime / 2f);
                (GameScreenElements[0] as Button).rectangle.Y = (int)(-(GameScreenElements[0] as Button).rectangle.Height * (1 - easing) + easing * initialY);
            }).AddAction_DeleteAfterTime(2f);



            (GameScreenElements[0] as Button).LeftButtonPressed += (RootScene.root_scene as GameScreen).PauseClick;




            //Elements.Add(new Slider(Manager)
            //{
            //    rectangle = RB.GetRect(0 - 100, 700, 0, 50, 3, 3, 3, 3),
            //    text = "Pause",
            //    textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
            //    scale = 0.3f,
            //    mainColor = Color.MonoGameOrange
            //});
        }


        public void SubscribeOnExit(Action action)
        {
            var act = action;
            (pauseElements[2] as ButtonEffected).LeftButtonPressed +=()=> { act();
                Thread.Sleep(1000);
            } ;
            
        }

        #region Pause
        List<DrawableUIElement> pauseElements = new List<DrawableUIElement>();
        private void CreatePauseGUI()
        {
            int _marginY = 10;
            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = RB.GetRect(500, 700, 100, 150, 0, 10, _marginY, _marginY),
                text = "Return",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.3f,
                mainColor = Color.Orange
            });
            (Elements.Last() as ButtonEffected).AddEffectEarly(new DUIE_Outline(Elements.Last()));
            pauseElements.Add(Elements.Last());

            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = RB.GetRect(500, 700, 150, 200, 0, 10, _marginY, _marginY),
                text = "Settings",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.3f,
                mainColor = Color.OrangeRed
            });
            (Elements.Last() as ButtonEffected).AddEffectEarly(new DUIE_Outline(Elements.Last()));
            pauseElements.Add(Elements.Last());

            Elements.Add(new ButtonEffected(Manager)
            {
                rectangle = RB.GetRect(500, 700, 200, 250, 0, 10, _marginY, _marginY),
                text = "Quit",
                textAligment = SDMonoUI.UI.Enums.TextAligment.Center,
                scale = 0.3f,
                mainColor = Color.DarkOrange
            });
            (Elements.Last() as ButtonEffected).AddEffectEarly(new DUIE_Outline(Elements.Last()));
            pauseElements.Add(Elements.Last());

            #region buttons setting



            (pauseElements[0] as Button).LeftButtonPressed += () =>
            {
                (RootScene.root_scene as GameScreen).ReturnToGame();
            };
            (pauseElements[2] as Button).LeftButtonPressed += () =>
            {
                RootScene.LoadScene(new MainMenu());
            };
            #endregion

            pause_baseY_temp_1 = pauseElements[0].rectangle.Y;
            pause_baseY_temp_2 = pauseElements[1].rectangle.Y;
            pause_baseY_temp_3 = pauseElements[2].rectangle.Y;

            foreach (var item in pauseElements)
            {
                item.rectangle.Y = -item.rectangle.Height - 100;
            }
        }


        int pause_baseY_temp_1 = 0;
        int pause_baseY_temp_2 = 0;
        int pause_baseY_temp_3 = 0;
        int pause_button_baseY_temp = 0;


        bool openingpause_Animation = false;
        public void StartAnimationOpenPause()
        {

            for (int i = 0; i < 3; i++)
                pauseElements[i].rectangle.Y = -pauseElements[i].rectangle.Y - 100;
            float duration = 1.7f;
            openingpause_Animation = true;
            new DataChanger((s, totalTime, dt) =>
            {
                openingpause_Animation = true;
                float easing = (float)Easings.EaseOutBack(totalTime / duration);
                (pauseElements[0] as Button).rectangle.Y = (int)(-((pauseElements[0] as Button).rectangle.Height + 50) * (1 - easing) + easing * pause_baseY_temp_1);
                easing = (float)Easings.EaseOutBack(totalTime / duration);
                (pauseElements[1] as Button).rectangle.Y = (int)(-((pauseElements[0] as Button).rectangle.Height + 50) * (1 - easing) + easing * pause_baseY_temp_2);
                easing = (float)Easings.EaseOutBack(totalTime / duration);
                (pauseElements[2] as Button).rectangle.Y = (int)(-((pauseElements[0] as Button).rectangle.Height + 50) * (1 - easing) + easing * pause_baseY_temp_3);
            }, (dc, timepassed) =>
            {
                //TODO disable buttons
            }).AddAction_DeleteAfterTime(duration);




            int temp_pause_but = GameScreenElements[0].rectangle.Y;
            new DataChanger((dc, timepassed, dt) =>
            {
                float easing = 1 - (float)Easings.EaseOutBack(timepassed / duration);
                (GameScreenElements[0] as Button).rectangle.Y = (int)(-((GameScreenElements[0] as Button).rectangle.Height + 50) * (1 - easing) + easing * temp_pause_but);
            }, (dc, timepassed) =>
            {
                //TODO disable buttons
            }).AddAction_DeleteAfterTime(duration);
        }
        public void StartAnimationClosePause()
        {

            int temp_1 = pauseElements[0].rectangle.Y;
            int temp_2 = pauseElements[1].rectangle.Y;
            int temp_3 = pauseElements[2].rectangle.Y;
            float duration = 1.5f;
            new DataChanger((dc, timepassed, dt) =>
            {
                float easing = 1 - (float)Easings.EaseOutBack(timepassed / duration);
                (pauseElements[0] as Button).rectangle.Y = (int)(-((pauseElements[0] as Button).rectangle.Height + 50) * (1 - easing) + easing * temp_1);
                easing = 1 - (float)Easings.EaseOutBack(timepassed / duration);
                (pauseElements[1] as Button).rectangle.Y = (int)(-((pauseElements[1] as Button).rectangle.Height + 50) * (1 - easing) + easing * temp_2);
                easing = 1 - (float)Easings.EaseOutBack(timepassed / duration);
                (pauseElements[2] as Button).rectangle.Y = (int)(-((pauseElements[2] as Button).rectangle.Height + 50) * (1 - easing) + easing * temp_3);
            }, (dc, timepassed) =>
            {
                //TODO disable buttons
            }).AddAction_DeleteAfterTime(duration);


            GameScreenElements[0].rectangle.Y = -GameScreenElements[0].rectangle.Y - 100;
            new DataChanger((s, totalTime, dt) =>
            {
                float easing = (float)Easings.EaseOutBack(totalTime / duration);
                (GameScreenElements[0] as Button).rectangle.Y = (int)(-((GameScreenElements[0] as Button).rectangle.Height + 50) * (1 - easing) + easing * pause_button_baseY_temp);
            }, (dc, timepassed) =>
            {
                //TODO disable buttons
            }).AddAction_DeleteAfterTime(duration);
        }
        #endregion
        public override void Update(float dt)
        {
            base.Update(dt);
        }

    }
}
