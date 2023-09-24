﻿using GLFW;
using Electron2D.Core;
using Electron2D.Core.Management;
using Electron2D.Build.Resources.Objects;
using Electron2D.Core.GameObjects;
using System.Numerics;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using System.Drawing;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Audio;
using Electron2D.Core.Physics;
using Electron2D.Core.UI;
using Electron2D.Core.Misc;

namespace Electron2D.Build
{
    public class MainGame : Game
    {
        private List<GameObject> environmentTiles = new List<GameObject>();

        public MainGame(int _initialWindowWidth, int _initialWindowHeight, string _initialWindowTitle) : base(_initialWindowWidth, _initialWindowHeight, _initialWindowTitle)
        {

        }

        protected override void Initialize()
        {  
            Console.WriteLine("Game Started");
        }

        protected override void Start()
        {
            // Environment Spritesheet
            ResourceManager.Instance.LoadTexture("Build/Resources/Textures/EnvironmentTiles.png");
            SpritesheetManager.Add(13, 11);

            //Random rand = new Random();
            //int environmentScale = 50;
            //int tiles = 5;
            //for (int x = -tiles; x <= tiles; x++)
            //{
            //    for (int y = -tiles; y <= tiles; y++)
            //    {
            //        GameObject tile = new GameObject(-1);
            //        tile.renderer = new BatchedSpriteRenderer(tile.transform);
            //        tile.transform.position = new Vector2(x, y) * environmentScale;
            //        tile.transform.scale = Vector2.One * environmentScale;
            //        environmentTiles.Add(tile);

            //        if(rand.Next(2) == 1)
            //        {
            //            tile.SetSprite(0, 2, 9);
            //        }
            //        else
            //        {
            //            tile.SetSprite(0, 3, 9);
            //        }
            //    }
            //}

            UiComponent ui = new TestUi(Color.White, 200, 100);
        }

        protected override void Update()
        {
            CameraMovement();
            Console.WriteLine($"FPS: {PerformanceTimings.framesPerSecond}");
        }

        private void SpawnNewPhysicsObj(Vector2 _position)
        {
            GameObject obj = new GameObject(-1, false);
            obj.renderer = new BatchedSpriteRenderer(obj.transform);
            obj.transform.position = _position;
            obj.SetSprite(0, 1, 0);

            VerletBody body = new VerletBody(obj.transform);
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 0.5f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.main.position += new Vector2(0, moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.main.position += new Vector2(-moveSpeed * Time.deltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.main.position += new Vector2(0, -moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.main.position += new Vector2(moveSpeed * Time.deltaTime, 0);
            }
        }

        protected unsafe override void Render()
        {

        }
    }
}
