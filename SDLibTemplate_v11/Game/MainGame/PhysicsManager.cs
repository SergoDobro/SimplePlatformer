using Microsoft.Xna.Framework;
using Simple_Platformer.Game.MainGame.Components;
using System;
using System.Collections.Generic;

namespace GameLogic;

[Flags]
public enum CollisionGroup
{
    None = 0,
    Group1 = 1 << 0,
    Group2 = 1 << 1,
    Group3 = 1 << 2,
    All = ~0
}

public struct Collider
{
    public Vector2 Offset;
    public Vector2 Size;
    public Microsoft.Xna.Framework.Rectangle ToRectangle() => new Rectangle(Offset.ToPoint(), Size.ToPoint());
    public Microsoft.Xna.Framework.Rectangle ToRectangle(float scale) => new Rectangle((Offset*scale).ToPoint(),(Size * scale).ToPoint());
}

public class PhysicsManager 
{
    public static PhysicsManager MainInstance { get; set; }
    public List<Rigidbody> _rigidbodies = new List<Rigidbody>();
    private Dictionary<(CollisionGroup, CollisionGroup), bool> _collisionMatrix = new Dictionary<(CollisionGroup, CollisionGroup), bool>();

    public PhysicsManager(bool isMain = true) { 
        if (isMain) 
            MainInstance = this; 
    }
    public void AddRigidbody(Rigidbody rb) => _rigidbodies.Add(rb);
    public void RemoveRigidbody(Rigidbody rb) => _rigidbodies.Remove(rb);

    public void SetCollision(CollisionGroup groupA, CollisionGroup groupB, bool canCollide)
    {
        _collisionMatrix[(groupA, groupB)] = canCollide;
        _collisionMatrix[(groupB, groupA)] = canCollide;
    }

    public void Update(float deltaTime)
    {
        ApplyPhysics(deltaTime);
        CheckCollisions();
    }

    public float friction = 0.01f;
    private void ApplyPhysics(float deltaTime)
    {
        foreach (var rb in _rigidbodies)
        {
            if (rb.IsKinematic) continue;

            rb.Velocity += rb.Acceleration * deltaTime;
            rb.Velocity *= 1 - friction;
            rb.Position += rb.Velocity * deltaTime;
            rb.ResetCollisionFlags();
        }
    }

    private void CheckCollisions()
    {
        Rigidbody a = null;
        Rigidbody b = null;
        for (int i = 0; i < _rigidbodies.Count; i++)
        {
            for (int j = i + 1; j < _rigidbodies.Count; j++)
            {
                a = _rigidbodies[i];
                b = _rigidbodies[j];


                //if ((a.Position - b.Position).LengthSquared() > 1000 * 100) break; //TODO maybe remove
                

                if (!ShouldCollide(a.Group, b.Group)) continue;

                foreach (var colliderA in a.Colliders)
                {
                    foreach (var colliderB in b.Colliders)
                    {
                        if (CheckColliderCollision(a, colliderA, b, colliderB, out var normal, out var depth))
                        {
                            ResolveCollision(a, b, normal, depth);
                            UpdateCollisionFlags(a, normal);
                            UpdateCollisionFlags(b, -normal);
                        }
                    }
                }
            }
        }
    }

    private bool ShouldCollide(CollisionGroup groupA, CollisionGroup groupB)
    {
        return _collisionMatrix.ContainsKey((groupA, groupB));
        return _collisionMatrix.TryGetValue((groupA, groupB), out var canCollide) && canCollide;

    }

    private bool CheckColliderCollision(Rigidbody a, Collider colliderA, Rigidbody b, Collider colliderB,
                                      out Vector2 normal, out float depth)
    {
        if ((a.Position - b.Position).LengthSquared()>50*50)
        {
            normal = new Vector2();
            depth = 0;
            return false;
        }

        normal = Vector2.Zero;
        depth = 0f;

        var obbA = GetWorldOBB(a, colliderA);
        var obbB = GetWorldOBB(b, colliderB);

        return SATTest(obbA, obbB, ref normal, ref depth);
    }

    private OBB GetWorldOBB(Rigidbody rb, Collider collider)
    {
        float cos = MathF.Cos(rb.Rotation);
        float sin = MathF.Sin(rb.Rotation);

        return new OBB(
            center: rb.Position + new Vector2(
                collider.Offset.X * cos - collider.Offset.Y * sin,
                collider.Offset.X * sin + collider.Offset.Y * cos),
            halfExtents: collider.Size * 0.5f,
            rotation: rb.Rotation
        );
    }

    private bool SATTest(OBB a, OBB b, ref Vector2 normal, ref float depth)
    {
        float minOverlap = float.MaxValue;
        Vector2 smallestAxis = Vector2.Zero;

        foreach (var axis in GetAxes(a, b))
        {
            Projection p1 = Project(a, axis);
            Projection p2 = Project(b, axis);

            if (!p1.Overlaps(p2)) return false;

            float overlap = p1.GetOverlap(p2);
            if (overlap < minOverlap)
            {
                minOverlap = overlap;
                smallestAxis = axis;
            }
        }

        Vector2 direction = b.Center - a.Center;
        if (Vector2.Dot(direction, smallestAxis) < 0)
            smallestAxis = -smallestAxis;

        normal = smallestAxis;
        depth = minOverlap;
        return true;
    }

    private List<Vector2> GetAxes(OBB a, OBB b)
    {
        return new List<Vector2>
        {
            a.Right,
            a.Up,
            b.Right,
            b.Up
        };
    }

    private Projection Project(OBB obb, Vector2 axis)
    {
        axis = Vector2.Normalize(axis);

        float proj = Vector2.Dot(obb.Center, axis);
        float radius = obb.HalfExtents.X * Math.Abs(Vector2.Dot(obb.Right, axis)) +
                      obb.HalfExtents.Y * Math.Abs(Vector2.Dot(obb.Up, axis));

        return new Projection(proj - radius, proj + radius);
    }
    private void ResolveCollision(Rigidbody a, Rigidbody b, Vector2 normal, float depth)
    {
        if (a.IsKinematic && b.IsKinematic) return;

        // Add small bias to prevent micro-collisions
        float bias = 0.001f;
        Vector2 resolution = normal * (depth + bias);

        // Position resolution (original code)
        if (a.IsKinematic)
        {
            b.Position += resolution;
        }
        else if (b.IsKinematic)
        {
            a.Position -= resolution;
        }
        else
        {
            a.Position -= resolution * 0.5f;
            b.Position += resolution * 0.5f;
        }

        // Improved velocity cancellation
        if (!a.IsKinematic)
        {
            // Only cancel velocity if moving into collision (negative dot product)
            float velDot = Vector2.Dot(a.Velocity , normal);
            if (velDot > 0)
            {
                // Cancel velocity component in collision direction
                a.Velocity -= Vector2.Dot(a.Velocity, normal) * normal;
            }
        }

        if (!b.IsKinematic)
        {
            float velDot = Vector2.Dot(b.Velocity + b.Acceleration, normal);
            if (velDot < 0)
            {
                b.Velocity -= Vector2.Dot(b.Velocity, -normal) * (-normal);
            }
        }
    }

    private void UpdateCollisionFlags(Rigidbody rb, Vector2 normal)
    {
        if (Math.Abs(normal.X) > Math.Abs(normal.Y))
        {
            if (normal.X > 0) rb.IsCollidingLeft = true;
            else rb.IsCollidingRight = true;
        }
        else
        {
            if (normal.Y > 0) rb.IsCollidingDown = true;
            else rb.IsCollidingUp = true;
        }
    }

    private struct OBB
    {
        public Vector2 Center;
        public Vector2 HalfExtents;
        public float Rotation;
        public Vector2 Right => new Vector2(MathF.Cos(Rotation), MathF.Sin(Rotation));
        public Vector2 Up => new Vector2(-Right.Y, Right.X);

        public OBB(Vector2 center, Vector2 halfExtents, float rotation)
        {
            Center = center;
            HalfExtents = halfExtents;
            Rotation = rotation;
        }
    }

    private struct Projection
    {
        public float Min;
        public float Max;

        public Projection(float min, float max) => (Min, Max) = (min, max);

        public bool Overlaps(Projection other) => Min < other.Max && other.Min < Max;
        public float GetOverlap(Projection other) => Math.Min(Max, other.Max) - Math.Max(Min, other.Min);
    }
}