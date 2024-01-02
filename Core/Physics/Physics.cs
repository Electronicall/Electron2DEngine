﻿using Box2DX.Collision;
using Box2DX.Dynamics;
using Box2DX.Common;
using System.Numerics;

namespace Electron2D.Core
{
    //https://box2d.org/documentation/md__d_1__git_hub_box2d_docs_hello.html

    public static class Physics
    {
        private static Dictionary<uint, Body> physicsBodies = new Dictionary<uint, Body>();

        private static World world;
        private static AABB aabb;

        /// <summary>
        /// This is called while the game is loading. Initializes the main physics world
        /// (Other physics worlds can be created separately from this using the Box2DX classes).
        /// </summary>
        /// <param name="_worldLowerBound"></param>
        /// <param name="_worldUpperBound"></param>
        /// <param name="_gravity"></param>
        /// <param name="_doSleep"></param>
        public static void Initialize(Vector2 _worldLowerBound, Vector2 _worldUpperBound, Vector2 _gravity, bool _doSleep)
        {
            aabb = new AABB()
            {
                LowerBound = new Vec2(_worldLowerBound.X, _worldLowerBound.Y),
                UpperBound = new Vec2(_worldUpperBound.X, _worldUpperBound.Y),
            };

            world = new World(aabb, new Vec2(_gravity.X, _gravity.Y), _doSleep);
        }

        /// <summary>
        /// Steps the physics simulation. This should only be called by the physics thread.
        /// </summary>
        /// <param name="_deltaTime"></param>
        /// <param name="_velocityIterations"></param>
        /// <param name="_positionIterations"></param>
        public static void Step(float _deltaTime, int _velocityIterations, int _positionIterations)
        {
            world.Step(_deltaTime, _velocityIterations, _positionIterations);
        }

        /// <summary>
        /// Creates a physics body and returns it's ID.
        /// </summary>
        /// <param name="_bodyDefinition">The definition of the physics body.</param>
        /// <param name="_polygonDefinition">The definition of the polygon. This determines the shape of the physics body.</param>
        /// <param name="_autoSetMass">If this is set to true, the physics body will use the shape and density to detemine the mass.
        /// This also makes the physics body dynamic.</param>
        /// <returns></returns>
        public static uint CreatePhysicsBody(BodyDef _bodyDefinition, PolygonDef _polygonDefinition, bool _autoSetMass = false)
        {
            Body b = world.CreateBody(_bodyDefinition);
            if(b == null)
            {
                Debug.LogError("PHYSICS: Error creating physics body!");
                return uint.MaxValue;
            }
            b.CreateFixture(_polygonDefinition);
            if (_autoSetMass) b.SetMassFromShapes();
            uint id = (uint)physicsBodies.Count;
            physicsBodies.Add(id, b);
            return id;
        }
        /// <summary>
        /// Creates a dynamic physics body and returns it's ID.
        /// </summary>
        /// <param name="_bodyDefinition">The definition of the physics body.</param>
        /// <param name="_polygonDefinition">The definition of the polygon. This determines the shape of the physics body.</param>
        /// <param name="_massData">The mass data of the physics body. By setting this, the physics body will be dynamic.</param>
        /// <returns></returns>
        public static uint CreatePhysicsBody(BodyDef _bodyDefinition, PolygonDef _polygonDefinition, MassData _massData)
        {
            Body b = world.CreateBody(_bodyDefinition);
            b.CreateFixture(_polygonDefinition);
            b.SetMass(_massData);
            uint id = (uint)physicsBodies.Count;
            physicsBodies.Add(id, b);
            return id;
        }

        /// <summary>
        /// Removes a physics body from the simulation.
        /// </summary>
        /// <param name="_id"></param>
        public static void RemovePhysicsBody(uint _id)
        {
            world.DestroyBody(physicsBodies[_id]);
        }

        /// <summary>
        /// Gets the position of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Vector2 GetBodyPosition(uint _id)
        {
            Vec2 vec = physicsBodies[_id].GetPosition();
            return new Vector2(vec.X, vec.Y);
        }

        /// <summary>
        /// Gets the rotation of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static float GetBodyRotation(uint _id)
        {
            return 180 / MathF.PI * physicsBodies[_id].GetAngle();
        }

        /// <summary>
        /// Gets the linear velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static Vector2 GetBodyVelocity(uint _id)
        {
            Vec2 vec = physicsBodies[_id].GetLinearVelocity();
            return new Vector2(vec.X, vec.Y);
        }

        /// <summary>
        /// Gets the angular velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public static float GetBodyAngularVelocity(uint _id)
        {
            return physicsBodies[_id].GetAngularVelocity();
        }

        /// <summary>
        /// Applies a force to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_force"></param>
        /// <param name="_point">The point the force is applied to.</param>
        public static void ApplyForce(uint _id, Vector2 _force, Vector2 _point)
        {
            physicsBodies[_id].ApplyForce(new Vec2(_force.X, _force.Y), new Vec2(_point.X, _point.Y));
        }

        /// <summary>
        /// Applies an impulse to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_impulse"></param>
        /// <param name="_point">The point the force is applied to.</param>
        public static void ApplyImpulse(uint _id, Vector2 _impulse, Vector2 _point)
        {
            physicsBodies[_id].ApplyImpulse(new Vec2(_impulse.X, _impulse.Y), new Vec2(_point.X, _point.Y));
        }

        /// <summary>
        /// Applies a torque to a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_torque"></param>
        public static void ApplyTorque(uint _id, float _torque)
        {
            physicsBodies[_id].ApplyTorque(_torque);
        }

        /// <summary>
        /// Sets the angle (in degrees) of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_angle"></param>
        public static void SetAngle(uint _id, float _angle)
        {
            physicsBodies[_id].SetAngle(_angle * (MathF.PI / 180));
        }

        /// <summary>
        /// Sets the angular velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_angle"></param>
        public static void SetAngularVelocity(uint _id, float _angularVelocity)
        {
            physicsBodies[_id].SetAngularVelocity(_angularVelocity);
        }

        /// <summary>
        /// Sets the linear velocity of a physics body.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_linearVelocity"></param>
        public static void SetLinearVelocity(uint _id, Vector2 _linearVelocity)
        {
            physicsBodies[_id].SetLinearVelocity(new Vec2(_linearVelocity.X, _linearVelocity.Y));
        }

        /// <summary>
        /// Sets the position of a physics body. Use this instead of <see cref="Transform.Position"/> so that movements are handled within the physics engine.
        /// </summary>
        /// <param name="_id"></param>
        /// <param name="_position"></param>
        public static void SetPosition(uint _id, Vector2 _position)
        {
            physicsBodies[_id].SetPosition(new Vec2(_position.X, _position.Y));
        }
    }
}
