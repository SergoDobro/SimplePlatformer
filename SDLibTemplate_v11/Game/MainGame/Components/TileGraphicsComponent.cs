using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Simple_Platformer.Game.MainGame.GameObjectSystem;

namespace Simple_Platformer.Game.MainGame.Components
{
    public class TileGraphicsComponent : GraphicsComponent, IRegistable
    {
        public int tilesheetPosId = Random.Shared.Next(0, 14);
        public void Draw(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures,
                    Vector2 cameraOffset, string sheetName, float scaleFactor = 1.0f, Color? color = null)
        {
            if (!textures.TryGetValue(sheetName, out Texture2D texture) || texture == null)
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

                float sca = 3f;
                // Calculate drawing parameters with global scaling
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 4f) / sca;
                Vector2 scale = new Vector2(
                    collider.Size.X / texture.Width * scaleFactor,
                    2 * collider.Size.Y / texture.Height * scaleFactor
                ) * sca;

                // Draw the collider with all transformations
                spriteBatch.Draw(
                    texture,
                    worldPosition,
                    TilingTexture.GetRectanlge(tilesheetPosId, texture.Width),
                    drawColor,
                    gameObject.Rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        public static List<TileGraphicsComponent> tileGraphicsComponents = new List<TileGraphicsComponent>();
        public void Register()
        {
            tileGraphicsComponents.Add(this);
        }

        public void GameObjectDestroyed()
        {
            tileGraphicsComponents.Remove(this);
        }

        public void ResetGlobal()
        {
            tileGraphicsComponents.Clear();
        }
    }
}