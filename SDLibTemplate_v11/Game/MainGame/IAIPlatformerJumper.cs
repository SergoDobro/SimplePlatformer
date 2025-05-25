using Microsoft.Xna.Framework;

namespace SDLibTemplate_v11.Game.MainGame
{
    internal interface IAIPlatformerJumper
    {
        public Vector2 GetClosestPlatformBeyondMyYX2(Vector2 position);
        public Vector2 GetClosestPlatformBeyondMyY(Vector2 position);
        public Vector2 GetClosestPlatformBelowMyY(Vector2 position);
    }
}