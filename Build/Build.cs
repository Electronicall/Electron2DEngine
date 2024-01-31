﻿using Electron2D.Core;
using Electron2D.Core.Audio;
using Electron2D.Core.Management;
using Electron2D.Core.Misc;
using Electron2D.Core.PhysicsBox2D;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Shaders;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.UserInterface;
using GLFW;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build
{
    public class Build : Game
    {
        private TextLabel fpsLabel;
        private UiComponent fpsBackground;
        private int displayFrames;
        private int frames;
        private float lastFrameCountTime;
        private AudioInstance spawnSound;
        private AudioInstance backgroundMusic;
        private float physicsHitCooldown = 0.05f;
        private float lastPhysicsHitTime = -10;

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}", _vsync: false, _antialiasing: false, _physicsPositionIterations: 4, _physicsVelocityIterations: 8, _errorCheckingEnabled: true) { }

        protected override void Load()
        {
            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------
            SetBackgroundColor(Color.FromArgb(255, 80, 80, 80));
            InitializeFPSLabel();
            
            Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.Navy));
            RigidbodyDynamicDef df = new RigidbodyDynamicDef()
            {
                Velocity = Vector2.UnitY * 10,
                Shape = RigidbodyShape.Box
            };
            s.AddComponent(Rigidbody.CreateDynamic(df));

            RigidbodyStaticDef sf = new RigidbodyStaticDef();
            Sprite b = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.White), 0, 6, 6);
            b.Transform.Position = new Vector2(40, -250f);
            b.AddComponent(Rigidbody.CreateStatic(sf));

            RigidbodyStaticDef sf2 = new RigidbodyStaticDef()
            {
                Bounciness = 0.5f
            };
            Sprite a = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.White), 0, 500, 30);
            a.Transform.Position = new Vector2(0, -450f);
            a.AddComponent(Rigidbody.CreateStatic(sf2));

            Sprite f = new Sprite(Material.Create(GlobalShaders.DefaultTexture, Color.FromArgb(50, 255, 255, 255)), 0, 30, 30, 10);
            f.Transform.Position = new Vector2(-50, -350);
            RigidbodySensor sensor = new RigidbodySensor(new Vector2(30), _shape: ColliderSensorShape.Box);
            f.AddComponent(sensor);
        }

        protected override void Update()
        {
            CameraMovement();
            CalculateFPS();

            if(Input.GetMouseButtonDown(MouseButton.Right))
            {
                CreateRigidbody();
            }

            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Item1.Transform.Scale = Vector2.One * 40 * Easing.EaseOutQuad(MathEx.Clamp01((Time.GameTime - sprites[i].Item2) * 20f));
            }
        }

        private List<(Sprite, float)> sprites = new List<(Sprite, float)>();
        private void CreateRigidbody()
        {
            Random rand = new Random();
            Sprite s = new Sprite(Material.Create(GlobalShaders.DefaultTexture,
                Color.FromArgb(255, rand.Next(0, 256), rand.Next(0, 256), rand.Next(0, 256))), 0, 40, 40);
            RigidbodyDynamicDef df = new RigidbodyDynamicDef()
            {
                Shape = RigidbodyShape.Box,
            };
            s.Transform.Position = Input.GetMouseWorldPosition();
            s.AddComponent(Rigidbody.CreateDynamic(df));
            spawnSound.Play();
            sprites.Add((s, Time.GameTime));

            s.GetComponent<Rigidbody>().OnBeginContact += (rb) => PlayRigidbodyHitSound(s.GetComponent<Rigidbody>());
        }

        private void PlayRigidbodyHitSound(Rigidbody _rb)
        {
            float _gameTime = Time.GameTime;
            float _lastTime = lastPhysicsHitTime + physicsHitCooldown;
            float magnitute = MathF.Abs(MathF.Sqrt((_rb.CalculatedVelocity.X * _rb.CalculatedVelocity.X)
                + (_rb.CalculatedVelocity.Y * _rb.CalculatedVelocity.Y)));
            float maxMagnitude = 40;
            float volume = MathEx.Clamp01(magnitute / maxMagnitude);
            volume = volume < 0.1f ? 0 : volume;

            if (_gameTime < _lastTime || volume == 0)
            {
                return;
            }
            lastPhysicsHitTime = _gameTime;

            // play physics hit
        }

        private void CameraMovement()
        {
            Camera2D.Main.Zoom += Input.ScrollDelta;
            Camera2D.Main.Zoom = Math.Clamp(Camera2D.Main.Zoom, 0.2f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.Main.Position += new Vector2(0, moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.Main.Position += new Vector2(-moveSpeed * Time.DeltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.Main.Position += new Vector2(0, -moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.Main.Position += new Vector2(moveSpeed * Time.DeltaTime, 0);
            }
        }

        private void InitializeFPSLabel()
        {
            fpsLabel = new TextLabel("FPS: 0", "Build/Resources/Fonts/OpenSans.ttf",
                30, Color.White, Color.White, new Vector2(130, 30), TextAlignment.Left, TextAlignment.Center,
                TextAlignmentMode.Geometry, TextOverflowMode.Disabled, _uiRenderLayer: 11);
            Material m = Material.Create(GlobalShaders.DefaultTexture, Color.FromArgb(60, 0, 0, 0), ResourceManager.Instance.LoadTexture("Build/Resources/Textures/white_circle.png"));
            fpsBackground = new SlicedUiComponent(m, 160, 40, 100, 100, 100, 100, 200, 0.2f);

            fpsLabel.Anchor = new Vector2(-1, 1);
            fpsLabel.Transform.Position = new Vector2((-1920 / 2) + 23, (1080 / 2) - 20);
            fpsBackground.Transform.Position = new Vector2((-1920 / 2) + 70 + 20, (1080 / 2) - 15 - 20);
        }

        private void CalculateFPS()
        {
            frames++;
            if (Time.GameTime - lastFrameCountTime >= 1)
            {
                lastFrameCountTime = Time.GameTime;
                displayFrames = frames;
                frames = 0;
            }

            fpsLabel.Text = $"FPS: {displayFrames}";
        }

        protected unsafe override void Render()
        {

        }
    }
}
