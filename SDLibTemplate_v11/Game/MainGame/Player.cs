using Microsoft.Xna.Framework;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class Player : GameObject
    {
        static float JumpPower = 14;
        public void ButtonUpPressed()
        {

            if (kayoteTime>0)
            {
                RigidBody.Velocity += new Vector2(0, -JumpPower);
            }
        }
        bool prevSt = false;
        float kayoteTime = 0;
        public void Update(float dt)
        {
            if (RigidBody.IsCollidingDown)
            {
                kayoteTime = 0.15f;
            }
            else
            {
                kayoteTime -= dt * 1;
            }
            prevSt = RigidBody.IsCollidingDown;
        }
    }
}