using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDMonoLibUtilits.Scenes;

namespace Simple_Platformer.Game.MainGame.Other
{

    public enum TransitionMode
    {
        Fade,
        Replace,
        Shader
    }

    public class ImageSlideshow : Scene
    {
        public string Path { get; set; } = "";

        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<Texture2D> _images = new List<Texture2D>();
        private Texture2D _currentTexture;
        private Texture2D _nextTexture;
        private Texture2D _whitePixel;

        private int _currentIndex;
        private int _nextIndex;
        private float _transitionProgress;
        private bool _isTransitioning;

        public Rectangle TargetRectangle { get; set; }
        public TransitionMode CurrentTransitionMode { get; set; } = TransitionMode.Fade;
        public float TransitionDuration { get; set; } = 1.0f;
        public bool ShadersEnabled { get; set; }
        public Effect TransitionEffect { get; set; }

        public ImageSlideshow(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            CreateWhitePixelTexture();
        }

        private void CreateWhitePixelTexture()
        {
            _whitePixel = new Texture2D(_graphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });
        }

        public override void LoadContent(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            LoadImages(Path, contentManager);
        }
        public void LoadImages(string contentPath, ContentManager content)
        {
            _images.Clear();

            var files = Directory.GetFiles(contentPath);
            foreach (var file in files)
            {
                if (file.EndsWith(".png") || file.EndsWith(".jpg"))
                {
                    using var stream = TitleContainer.OpenStream(file);
                    var texture = Texture2D.FromStream(_graphicsDevice, stream);
                    _images.Add(texture);
                }
            }

            if (_images.Count > 0)
                _currentTexture = _images[0];
        }

        public void NextImage()
        {
            if (_images.Count < 2 || _isTransitioning) return;

            _nextIndex = (_currentIndex + 1) % _images.Count;
            _nextTexture = _images[_nextIndex];
            _isTransitioning = true;
            _transitionProgress = 0f;

            if (CurrentTransitionMode == TransitionMode.Replace)
                CompleteTransitionImmediately();
        }

        private void CompleteTransitionImmediately()
        {
            _currentIndex = _nextIndex;
            _currentTexture = _nextTexture;
            _isTransitioning = false;
        }

        public override void Update(float dt)
        {
            if (!_isTransitioning) return;

            _transitionProgress += dt / TransitionDuration;

            if (_transitionProgress >= 1.0f)
            {
                _currentIndex = _nextIndex;
                _currentTexture = _nextTexture;
                _isTransitioning = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_currentTexture == null) return;

            if (_isTransitioning)
            {
                DrawTransition(spriteBatch);
            }
            else
            {
                spriteBatch.Begin();
                spriteBatch.Draw(_currentTexture, TargetRectangle, Color.White);
                spriteBatch.End();
            }
        }

        private void DrawTransition(SpriteBatch spriteBatch)
        {
            switch (CurrentTransitionMode)
            {
                case TransitionMode.Fade:
                    DrawFadeTransition(spriteBatch);
                    break;

                case TransitionMode.Shader when ShadersEnabled && TransitionEffect != null:
                    DrawShaderTransition(spriteBatch);
                    break;

                case TransitionMode.Shader:
                case TransitionMode.Replace:
                    DrawImmediateTransition(spriteBatch);
                    break;
            }
        }

        private void DrawFadeTransition(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(blendState: BlendState.AlphaBlend);
            spriteBatch.Draw(_currentTexture, TargetRectangle, Color.White * (1 - _transitionProgress));
            spriteBatch.Draw(_nextTexture, TargetRectangle, Color.White * _transitionProgress);
            spriteBatch.End();
        }

        private void DrawShaderTransition(SpriteBatch spriteBatch)
        {
            TransitionEffect.Parameters["Progress"]?.SetValue(_transitionProgress);
            TransitionEffect.Parameters["CurrentTexture"]?.SetValue(_currentTexture);
            TransitionEffect.Parameters["NextTexture"]?.SetValue(_nextTexture);

            spriteBatch.Begin(effect: TransitionEffect);
            spriteBatch.Draw(_whitePixel, TargetRectangle, Color.White);
            spriteBatch.End();
        }

        private void DrawImmediateTransition(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(_nextTexture, TargetRectangle, Color.White);
            spriteBatch.End();
        }


    }
}