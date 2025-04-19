using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDLibTemplate_v11.Game.MainGame;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameLogic;

public class Rigidbody : SDLibTemplate_v11.Game.MainGame.GameComponent, IRegistable
{ 

    [JsonIgnore]
    public Vector2 Position { get { return gameObject.Position; } set { gameObject.Position = value; } }
    [JsonIgnore]
    public float Rotation { get { return gameObject.Rotation; } set { gameObject.Rotation = value; } }
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }
    public List<Collider> Colliders { get; } = new List<Collider>();
    public CollisionGroup Group { get; set; }
    public bool IsKinematic { get; set; }

    public bool IsCollidingDown { get; set; }
    public bool IsCollidingUp { get; set; }
    public bool IsCollidingLeft { get; set; }
    public bool IsCollidingRight { get; set; }

    public void ResetCollisionFlags()
    {
        IsCollidingDown = false;
        IsCollidingUp = false;
        IsCollidingLeft = false;
        IsCollidingRight = false;
    }
    public void Draw(SpriteBatch spriteBatch, Dictionary<string, Texture2D> textures,
                Vector2 cameraOffset, float scaleFactor = 1.0f, Color? color = null)
    {
        if (!textures.TryGetValue("none", out Texture2D texture) || texture == null)
            return;

        Color drawColor = color ?? Color.White;

        foreach (var collider in Colliders)
        {
            // Calculate world position with rotation and camera offset
            float cos = MathF.Cos(Rotation);
            float sin = MathF.Sin(Rotation);
            Vector2 rotatedOffset = new Vector2(
                collider.Offset.X * cos - collider.Offset.Y * sin,
                collider.Offset.X * sin + collider.Offset.Y * cos
            );
            Vector2 worldPosition = (Position + rotatedOffset - cameraOffset) * scaleFactor;

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
                Rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }

    [JsonIgnore]
    public PhysicsManager PhysicsManager { get; set; }
    public void Register()
    {
        if (PhysicsManager is null)
            PhysicsManager = PhysicsManager.MainInstance;
        PhysicsManager.AddRigidbody(this);
    }

    public void GameObjecctDestroyed()
    {
        PhysicsManager.RemoveRigidbody(this);
    }
}
