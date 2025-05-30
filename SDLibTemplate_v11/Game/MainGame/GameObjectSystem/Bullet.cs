using GameLogic;
using Microsoft.Xna.Framework;
using Simple_Platformer.Game.MainGame.Components;

namespace Simple_Platformer.Game.MainGame.GameObjectSystem
{
    public class Bullet : GameObject
    {
        public Vector2 main_direction;
        public Bullet()
        {
            AddComponent(new Rigidbody
            {
                Group = CollisionGroup.Group3,
                Colliders = { new Collider { Offset = Vector2.Zero, Size = new Vector2(0.5f, 1f) } }
            });
        }
        public override void Update(float dt)
        {
            RigidBody.Velocity = main_direction;
            base.Update(dt);
        }

    }
}