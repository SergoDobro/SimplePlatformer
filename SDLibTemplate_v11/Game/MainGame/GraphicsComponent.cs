using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class GraphicsComponent : GameComponent
    { 
        public void Draw(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures,
                    Vector2 cameraOffset, float scaleFactor = 1.0f, Color? color = null)
        {
            if (!textures.TryGetValue("none", out Texture2D texture) || texture == null)
                return;

            Color drawColor = color ?? Color.White;


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
    }
}