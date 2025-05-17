using GameLogic;
using Microsoft.Xna.Framework;
using Simple_Platformer.Game.MainGame.Components;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Simple_Platformer.Game.MainGame.GameObjectSystem
{
    public class Player : GameObject
    {
        static float JumpPower = 1500;
        public void ButtonUpPressed()
        {

            if (kayoteTime > 0)
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
            tickWaiters[3] = false;
            pushreload -= dt;
        }
        bool[] tickWaiters = new bool[] { false, false, false, false };

        public void ButtonRightPressed()
        {
            if (!tickWaiters[0])
                RigidBody.Velocity += new Vector2(400, 0) * _dt;
            tickWaiters[0] = true;
            facingRight = true;
        }
        public void ButtonLeftPressed()
        {
            if (!tickWaiters[1])
                RigidBody.Velocity += new Vector2(-400, 0) * _dt;
            tickWaiters[1] = true;
            facingRight = false;
        }

        float pushreload = 0;
        bool facingRight = true;
        public void ButtonPush()
        {
            if (!tickWaiters[3] && pushreload <= 0)
                RigidBody.Velocity += new Vector2(Math.Sign(RigidBody.Velocity.X) * 400, 0) * _dt;
            pushreload = 1;
            tickWaiters[3] = true;
        }

        public void LaunchShot()
        {
            Instantiate(new Bullet()
            {
                Position = Position,
                main_direction = new Vector2(
                facingRight ? 1 : -1
                ,
                0) * 55
            });
        }
    }
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