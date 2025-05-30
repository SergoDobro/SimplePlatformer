using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Simple_Platformer.Game.MainGame.GameObjectSystem;
using SDMonoUI.UI.Elements;

namespace Simple_Platformer.Game.MainGame.Components
{

    public interface IGlobalRegisterable<T>
    {
        static List<T> Instances { get; } = new();

        void Register() => Instances.Add((T)this);
        void Unregister() => Instances.Remove((T)this);
        static void Reset() => Instances.Clear();
    }


    public class Texture2DComponent : GraphicsComponent, IRegistable//, IGlobalRegisterable<Texture2DComponent>
    {
        public string textureName { get; set; }
        public RectangleF rectangle { get; set; }
        public override void Draw(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures,
                    Vector2 cameraOffset, float scaleFactor = 1.0f, Color? color = null)
        {
            if (!textures.TryGetValue(textureName, out Texture2D texture) || texture == null)
                return;

            Color drawColor = color ?? Color.White;

            // Calculate world position with rotation and camera offset
            float cos = MathF.Cos(gameObject.Rotation);
            float sin = MathF.Sin(gameObject.Rotation);

            Vector2 worldPosition = (gameObject.Position  - cameraOffset) * scaleFactor;

            // Calculate drawing parameters with global scaling
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Vector2 scale = new Vector2(
                rectangle.Size.X / texture.Width * scaleFactor,
                rectangle.Size.Y / texture.Height * scaleFactor
            );

            // Draw the collider with all transformations
            spriteBatch.Draw(
                texture,
                worldPosition,
                null,
                drawColor,
                gameObject.Rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );

        }

        public static List<Texture2DComponent> AllInstances = new List<Texture2DComponent>();
        public void Register()
        {
            AllInstances.Add(this);
        }

        public void GameObjectDestroyed()
        {
            AllInstances.Remove(this);
        }

        public void ResetGlobal()
        {
            AllInstances.Clear();
        }
    }
}