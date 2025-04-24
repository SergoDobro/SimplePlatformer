using Microsoft.Xna.Framework;
using System;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class Player : GameObject
    {
        static float JumpPower = 1500;
        public void ButtonUpPressed()
        {

            if (kayoteTime>0)
            {
                if (!tickWaiters[2])
                    RigidBody.Velocity += new Vector2(0, -JumpPower) * _dt;
            }
            tickWaiters[2] = true;
        }
        public float _dt;
        bool prevSt = false;
        float kayoteTime = 0;
        public void Update(float dt)
        {
            _dt = dt;
            if (RigidBody.IsCollidingDown)
            {
                kayoteTime = 0.1f;// 0.15f;
            }
            else
            {
                kayoteTime -= dt * 1;
            }
            prevSt = RigidBody.IsCollidingDown;
            
            tickWaiters[0] = false;
            tickWaiters[1] = false;
            tickWaiters[2] = false;
        }
        bool[] tickWaiters = new bool[3] { false, false, false};

        public void ButtonRightPressed()
        {
            if (!tickWaiters[0])
                RigidBody.Velocity += new Vector2(400, 0) * _dt;
            tickWaiters[0] = true;
        }
        public void ButtonLeftPressed()
        {
            if (!tickWaiters[1])
                RigidBody.Velocity += new Vector2(-400, 0) * _dt;
            tickWaiters[1] = true;
        }
    }
}