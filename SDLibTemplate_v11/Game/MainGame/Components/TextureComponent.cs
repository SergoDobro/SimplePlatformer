using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDLibTemplate_v11.Game.MainGame;
using SDMonoLibUtilits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GameComponent = SDLibTemplate_v11.Game.MainGame.GameComponent;

namespace Simple_Platformer.Game.MainGame.Components
{
    public class GraphicsComponentExtended : GraphicsComponent, IRegistable
    {
        bool Loaded = false;
        Effect eff;
        public Color main_color = new Color((uint)Random.Shared.Next(int.MinValue+1, int.MaxValue-1));
        public float blendPower = 0.8f;
        public void Load()
        {
            Loaded = true;

        } 
        public void Draw(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures,
                    Vector2 cameraOffset, float scaleFactor = 1.0f, Color? color = null)
        {
            if (!Loaded)
            {
                Load();
            }

            if (!textures.TryGetValue("none", out Texture2D texture) || texture == null)
                return;

            Color drawColor = color ?? Color.White;
            drawColor = Color.FromNonPremultiplied(main_color.ToVector4() * blendPower
                + drawColor.ToVector4() * (1 - blendPower));


            foreach (var collider in gameObject.RigidBody.Colliders)
            {
                // Calculate world position with rotation and camera offset
                float cos = MathF.Cos(gameObject.Rotation);
                float sin = MathF.Sin(gameObject.Rotation);
                Vector2 rotatedOffset = new Vector2(
                    collider.Offset.X * cos - collider.Offset.Y * sin,
                    collider.Offset.X * sin + collider.Offset.Y * cos
                );
                Vector2 worldPosition = (gameObject.Position + rotatedOffset - cameraOffset) * scaleFactor;

                // Calculate drawing parameters with global scaling
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
                Vector2 scale = new Vector2(
                    (collider.Size.X / texture.Width) * scaleFactor,
                    (collider.Size.Y / texture.Height) * scaleFactor
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
        }

        public static List<GraphicsComponentExtended> GraphicsExtendedComponents = new List<GraphicsComponentExtended>();
        public void Register()
        {
            GraphicsExtendedComponents.Add(this);
        }

        public void GameObjectDestroyed()
        {
            GraphicsExtendedComponents.Remove(this);
        }
    }
}
