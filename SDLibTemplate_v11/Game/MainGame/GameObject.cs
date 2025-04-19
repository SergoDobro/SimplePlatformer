using GameLogic;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class GameComponent
    {
        public GameObject gameObject { get; set; }

    }
    public interface IRegistable
    {
        public void Register(GameComponent gameComponent);
    }
    public class GameObject
    {

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public Rigidbody RigidBody { get; set; }
        public List<GameComponent> gameComponents { get; set; } = new List<GameComponent>();
        public void AddComponent(GameComponent gameComponent)
        {
            gameComponents.Add(gameComponent);
            if (gameComponent is Rigidbody)
            {
                RigidBody = gameComponent as Rigidbody;
            }
            if (gameComponent is IRegistable)
            {
                (gameComponent as IRegistable).Register(gameComponent);
            }
            gameComponent.gameObject = this;
        }
        // TODO: Add a 'Category' and 'Id' property here if we need to map objects to dictionaries
        // [JsonIgnore] // Uncomment if adding these later
        // public string Category { get; set; } = "Misc";
        //public string Id { get; set; } = Guid.NewGuid().ToString();
    }
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
    public class TileGraphicsComponent : GraphicsComponent, IRegistable
    {
        public int tilesheetPosId = Random.Shared.Next(0,14);
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
                Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 4f)/ sca;
                Vector2 scale = new Vector2(
                    (collider.Size.X / texture.Width) * scaleFactor,
                    (2*collider.Size.Y / texture.Height) * scaleFactor
                )* sca;

                // Draw the collider with all transformations
                spriteBatch.Draw(
                    texture,
                    worldPosition,
                    TilingTexture.GetRectanlge(tilesheetPosId, (int)(texture.Width)),
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
        public void Register(GameComponent gameComponent)
        {
            tileGraphicsComponents.Add(gameComponent as TileGraphicsComponent);
        }
    }
    public class TilingTexture
    {
        public static Dictionary<string, Texture2D> SpriteSheets { get; private set; }
        public static Rectangle GetRectanlge(int id, int textureScale)
        {
            Point[] shifts = new Point[] { 
            new Point(0,0),
            new Point(1,0),
            new Point(2,0),
            new Point(0,1),
            new Point(1,1),
            new Point(2,1),
            new Point(0,2),
            new Point(1,2),
            new Point(2,2),

            new Point(1,3),
            new Point(0,4),
            new Point(1,4),
            new Point(2,4),
            new Point(1,5),

            };
            Rectangle rect = new Rectangle(shifts[id].X * textureScale/3,
                shifts[id].Y * textureScale/3, textureScale / 3, textureScale/3);
            return rect;
        }
        public static int ConvertToIndex(bool groundLeft, bool groundTop, bool groundRight, bool groundBottom)
        {
            if (groundLeft && groundTop && !groundRight && !groundBottom)
                return 0;
            if (!groundLeft && groundTop && !groundRight && !groundBottom)
                return 1;
            if (!groundLeft && groundTop && groundRight && !groundBottom)
                return 2;
            if (groundLeft && !groundTop && !groundRight && !groundBottom)
                return 3;
            if (!groundLeft && !groundTop && !groundRight && !groundBottom)
                return 4;
            if (!groundLeft && !groundTop && groundRight && !groundBottom)
                return 5;
            if (groundLeft && !groundTop && !groundRight && groundBottom)
                return 6;
            if (!groundLeft && !groundTop && !groundRight && groundBottom)
                return 7;
            if (!groundLeft && !groundTop && groundRight && groundBottom)
                return 8;

            if (groundLeft && groundTop && groundRight && !groundBottom)
                return 9;
            if (groundLeft && groundTop && !groundRight && groundBottom)
                return 10;
            if (groundLeft && groundTop && groundRight && groundBottom)
                return 11;
            if (!groundLeft && groundTop && groundRight && groundBottom)
                return 12;
            if (groundLeft && !groundTop && groundRight && groundBottom)
                return 13;
            return 4;
            /*
             *111
             *111
             *111
             *010
             *111
             *010
             *
             */
        }

    }
}