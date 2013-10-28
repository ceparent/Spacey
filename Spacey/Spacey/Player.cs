using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
namespace Spacey
{
    public class Player
    {
        float CameraSpeed = 0.003f;
        float WalkSpeed = 0.4f;
        float friction = 0.05f;
        public Vector3 _Gravity = Vector3.Down / 16;
        public Vector3 _velocity;
        public bool IsCrouching = false;

        public Vector3 StartPosition { get; set; }

        public BoundingBox boundingBox
        {
            get
            {
                float sides = 0.0001f;
                return new BoundingBox(feetPosition + new Vector3(-sides, 0, -sides), HeadPosition + new Vector3(sides, 0, sides));
            }

        }

        private Vector3 _position;
        public Vector3 HeadPosition
        {
            get
            {
                if (IsCrouching)
                    return _position + new Vector3(0, -2f,0);
                return _position;
            }

            set
            {
                if (IsCrouching)
                    _position = value + new Vector3(0, 2f, 0);
                else
                    _position = value;
            }
        }
        public Vector3 centerPosition
        {
            get
            {
                return _position + new Vector3(0, -2.5f, 0);
            }

            set
            {
                _position = value + new Vector3(0, 2.5f, 0);
            }
        }
        public Vector3 feetPosition
        {
            get
            {
                return _position + new Vector3(0, -5f, 0);
            }

            set
            {
                _position = value + new Vector3(0, 5f, 0);
            }
        }

        private Game Game;
        public Player(Game pGame)
        {
            Game = pGame;

            StartPosition = new Vector3(0, 15, 0);
            _position = StartPosition;

            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            originalMouse = new MouseState(Game.Window.ClientBounds.Width / 2,Game.Window.ClientBounds.Height / 2, 0, ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released,ButtonState.Released);


            _gunShot = Game.Content.Load<SoundEffect>("Sounds/gunshot");
        }
        MouseState originalMouse;


        



        private KeyboardState oldKs;
        private MouseState oldMs;
        public void Update()
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));

            Vector3 originalPosition = HeadPosition;
            

            //view
            if (ms != originalMouse && Game.IsActive)
            {
                float xdiff = ms.X - originalMouse.X;
                float ydiff = ms.Y - originalMouse.Y;

                Vector2 diff = new Vector2(-xdiff * CameraSpeed, -ydiff * CameraSpeed);
                
                camera.Rotation += diff;
                Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            }

            //Movement
            Vector3 movement = Vector3.Zero;

            if (ks.IsKeyDown(Keys.W))
                movement += Vector3.Forward;
            if (ks.IsKeyDown(Keys.S))
                movement += Vector3.Backward;
            if (ks.IsKeyDown(Keys.A))
                movement += Vector3.Left;
            if (ks.IsKeyDown(Keys.D))
                movement += Vector3.Right;

            if (movement != Vector3.Zero)
                movement.Normalize();

            

            //Jump
            if (oldKs.IsKeyUp(Keys.Space) && ks.IsKeyDown(Keys.Space) && _velocity.Y == 0)
            {
                _velocity.Y += 1.2f;

            }

            //Grav

            if (ks.IsKeyDown(Keys.Up) && oldKs.IsKeyUp(Keys.Up))
                _Gravity *= 2;
            if (ks.IsKeyDown(Keys.Down) && oldKs.IsKeyUp(Keys.Down))
                _Gravity /= 2;


            _velocity += _Gravity;
            _velocity *= 1 - friction;
            HeadPosition += _velocity;

            ITerrainComponent terrain = (ITerrainComponent)Game.Services.GetService(typeof(ITerrainComponent));


            
            //Debug

            if (ks.IsKeyDown(Keys.Enter))
                _position = StartPosition;

            Vector3 rotatedMov;
            Ray centerRay;
            

            movement = MovementCollision(camera, movement, terrain, out rotatedMov, out centerRay);



            //Crouching
            //TODO: fix stuttering when jumping down from block without pressing control and touching a ceiling
            if (!IsCrouching && ks.IsKeyDown(Keys.LeftControl))
                IsCrouching = true;
            if (IsCrouching && ks.IsKeyUp(Keys.LeftControl))
                IsCrouching = false;

            float speedFactor = 1;
            //SpeedFactor
            if (IsCrouching)
                speedFactor = 1f;
            else if (ks.IsKeyDown(Keys.LeftShift))
                speedFactor = 1.5f;




            _position += rotatedMov * WalkSpeed * speedFactor;

            centerRay = new Ray(HeadPosition, Vector3.Up);
            float tolerance = 1.5f;
            BoundingBox? result2 = RayTestGetBox(terrain, centerRay, tolerance);
            if (result2 != null)
            {
                _velocity.Y = -0.01f;
                HeadPosition = new Vector3(HeadPosition.X, result2.Value.Min.Y - 1.5f, HeadPosition.Z);
                IsCrouching = true;
            }



            
            if (_velocity.Y < 0)
            {
                FallCollision(terrain, ref rotatedMov, ref tolerance);
            }


            //Save player

            if (_position.Y < -200)
                _position = StartPosition;

            if (ms.LeftButton == ButtonState.Pressed && oldMs.LeftButton == ButtonState.Released)
            {
                Shoot(camera,terrain);
            }
            

            //Shader

            if (ks.IsKeyDown(Keys.E) && oldKs.IsKeyUp(Keys.E))
                terrain.ShaderIndex++;
            if (terrain.ShaderIndex > 1)
                terrain.ShaderIndex = 0;


            oldKs = ks;
            oldMs = ms;
        }

        public SoundEffect _gunShot;
        private void Shoot(ICameraComponent camera, ITerrainComponent terrain)
        {
            Ray shotRay = new Ray(HeadPosition, camera.RotatedTarget);
            BoundingBox? result = RayTestGetBox(terrain, shotRay, 500f);
            if (result != null)
            {
                Vector3 upDirection = Vector3.Up;
                Vector3 rot = camera.RotatedTarget;
                Vector3 direction = DetermineSide(result.Value, shotRay);
                if (direction == Vector3.Down)
                    upDirection = -camera.RotatedTarget;
                else if (direction == Vector3.Up)
                    upDirection = camera.RotatedTarget;

                float distance = result.Value.Intersects(shotRay).Value;
                Decal decal = new Decal(HeadPosition + rot * (distance - 0.1f), direction, upDirection, Game.Content.Load<Texture2D>("Textures/bullet_hole"));
                terrain.AddDecal(decal);
                _gunShot.Play();
            }


        }

        Vector3[] directions = { Vector3.Left, Vector3.Right, Vector3.Down, Vector3.Up, Vector3.Forward, Vector3.Backward };
        private Vector3 DetermineSide(BoundingBox box, Ray ray)
        {

            int selectedFace = 0;
            float? closestDist = float.MaxValue;
            BoundingBox[] sides = new BoundingBox[6];
            // -X  
            sides[0] = new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Min.X, box.Max.Y, box.Max.Z));

            // +X  
            sides[1] = new BoundingBox(new Vector3(box.Max.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z));

            // -Y  
            sides[2] = new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Min.Y, box.Max.Z));

            // +Y  
            sides[3] = new BoundingBox(new Vector3(box.Min.X, box.Max.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z));

            // -Z  
            sides[4] = new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Min.Z), new Vector3(box.Max.X, box.Max.Y, box.Min.Z));

            // +Z  
            sides[5] = new BoundingBox(new Vector3(box.Min.X, box.Min.Y, box.Max.Z), new Vector3(box.Max.X, box.Max.Y, box.Max.Z));

            for (int i = 0; i < 6; i++)
            {
                float? d = ray.Intersects(sides[i]);
                if (d.HasValue)
                {
                    if (d.Value < closestDist)
                    {
                        closestDist = d;
                        selectedFace = i;
                    }
                }
            }
            return directions[selectedFace];
        } 

        private void FallCollision(ITerrainComponent terrain, ref Vector3 rotatedMov, ref float tolerance)
        {
            Vector3 LeftFoot = feetPosition + new Vector3(0.5f, 0.2f, 0.5f) + rotatedMov;
            Vector3 RightFoot = feetPosition + new Vector3(-0.5f, 0.2f, 0.5f) + rotatedMov;
            Vector3 Centerfoot = feetPosition + new Vector3(0, 0.2f, 0) + rotatedMov;
            Vector3 Direction = Vector3.Down;
            Ray LeftFootRay = new Ray(LeftFoot, Direction);
            Ray RightFootRay = new Ray(RightFoot, Direction);
            Ray CenterFootRay = new Ray(Centerfoot, Direction);
            tolerance = 0.2f;
            BoundingBox? resultLeftFoot = RayTestGetBox(terrain, LeftFootRay, tolerance);
            BoundingBox? resultRightFoot = RayTestGetBox(terrain, RightFootRay, tolerance);
            BoundingBox? resultCenterFoot = RayTestGetBox(terrain, CenterFootRay, tolerance);
            if (resultCenterFoot != null)
            {
                _velocity.Y = 0f;
                feetPosition = new Vector3(feetPosition.X, resultCenterFoot.Value.Max.Y, feetPosition.Z);

            }
            else if (resultLeftFoot != null)
            {
                _velocity.Y = 0f;
                feetPosition = new Vector3(feetPosition.X, resultLeftFoot.Value.Max.Y, feetPosition.Z);

            }
            else if (resultRightFoot != null)
            {
                _velocity.Y = 0f;
                feetPosition = new Vector3(feetPosition.X, resultRightFoot.Value.Max.Y, feetPosition.Z);

            }
        }

        private Vector3 MovementCollision(ICameraComponent camera, Vector3 movement, ITerrainComponent terrain, out Vector3 rotatedMov, out Ray centerRay)
        {

            rotatedMov = camera.RotateDirectionWithoutY(movement);

            //Normalize movement
            if (rotatedMov != Vector3.Zero)
                rotatedMov.Normalize();
            rotatedMov.Y = 0;

            Vector3 futureCPosition = centerPosition + rotatedMov;
            Vector3 futureHPosition = HeadPosition + new Vector3(0, 2.5f, 0) + rotatedMov;

            Ray headRay;
            float? Cresult = null;
            float? Hresult = null;
            centerRay = new Ray(futureCPosition, new Vector3(-1, 0, 0));
            headRay = new Ray(futureHPosition, new Vector3(-1, 0, 0));
            Cresult = RayTest(terrain, centerRay);
            if (!IsCrouching)
                Hresult = RayTest(terrain, headRay);
            if (Cresult != null || Hresult != null)
                rotatedMov.X = 0;

            centerRay = new Ray(futureCPosition, new Vector3(1, 0, 0));
            headRay = new Ray(futureHPosition, new Vector3(1, 0, 0));
            Cresult = RayTest(terrain, centerRay);
            if (!IsCrouching)
                Hresult = RayTest(terrain, headRay);
            if (Cresult != null || Hresult != null)
                rotatedMov.X = 0;

            centerRay = new Ray(futureCPosition, new Vector3(0, 0, -1));
            headRay = new Ray(futureHPosition, new Vector3(0, 0, -1));
            Cresult = RayTest(terrain, centerRay);
            if (!IsCrouching)
                Hresult = RayTest(terrain, headRay);
            if (Cresult != null || Hresult != null)
                rotatedMov.Z = 0;


            centerRay = new Ray(futureCPosition, new Vector3(0, 0, 1));
            headRay = new Ray(futureHPosition, new Vector3(0, 0, 1));
            Cresult = RayTest(terrain, centerRay);
            if (!IsCrouching)
                Hresult = RayTest(terrain, headRay);
            if (Cresult != null || Hresult != null)
                rotatedMov.Z = 0;

            return movement;
        }

        private float? RayTest(ITerrainComponent terrain,Ray moveRay)
        {
            float? nearestFloat = null;
            foreach (BoundingBox box in terrain.BoundingBoxes)
            {
                float? distance = box.Intersects(moveRay);
                if (distance.HasValue && distance.Value < 1f)
                {
                    if (distance < nearestFloat)
                        nearestFloat = distance;

                    return distance;
                }
            }

            return nearestFloat;
        }
        private BoundingBox? RayTestGetBox(ITerrainComponent terrain, Ray moveRay, float pDistance)
        {
            BoundingBox? Nearestbox = null;
            float nearest = float.MaxValue;
            foreach (BoundingBox box in terrain.BoundingBoxes)
            {
                float? distance = box.Intersects(moveRay);
                if (distance.HasValue && distance.Value < pDistance)
                {
                    if (distance < nearest)
                    {
                        nearest = distance.Value;
                        Nearestbox = box;
                    }
                    
                }
            }

            return Nearestbox;
        }



    }
}
