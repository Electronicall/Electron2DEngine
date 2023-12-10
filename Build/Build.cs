﻿using Electron2D.Core;
using Electron2D.Core.Misc;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Text;
using Electron2D.Core.UI;
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

        public Build(int _initialWindowWidth, int _initialWindowHeight) : base(_initialWindowWidth, _initialWindowHeight,
            $"Electron2D Build - {Program.BuildDate}") { }

        protected override void Load()
        {
            SetBackgroundColor(Color.LightBlue);

            // Load Custom Component Systems
            // Ex. ComponentSystem.Start();
            // -----------------------------

            //fpsBackground = new Panel(Color.DarkGray, 10, 140, 30, true);
            //fpsLabel = new TextLabel("FPS: 0", "Build/Resources/Fonts/OpenSans.ttf",
            //    30, Color.White, Color.White, new Vector2(130, 30), TextAlignment.Left, TextAlignment.Center,
            //    TextAlignmentMode.Geometry, TextOverflowMode.Disabled, _uiRenderLayer: 11);
            //UiConstraint constraint = new PixelConstraint(20, UiConstraintSide.Left);
            //UiConstraint constraint2 = new PixelConstraint(20, UiConstraintSide.Top);
            //fpsLabel.Constraints.SetPosition(constraint);
            //fpsLabel.Constraints.SetPosition(constraint2);
            //fpsBackground.Constraints.SetPosition(constraint);
            //fpsBackground.Constraints.SetPosition(constraint2);


            Panel mainPanel = new Panel(Color.DarkGray, -1, 300, 500);
            VerticalLayout layout = new VerticalLayout(new Vector4(20), 20);
            mainPanel.AddComponent(layout);
            layout.AddToLayout(new TextLabel("This is a test of the vertical layout group with text.", "Build/Resources/Fonts/OpenSans.ttf",
                30, Color.White, Color.White, new Vector2(130, 30), TextAlignment.Center, TextAlignment.Center,
                TextAlignmentMode.Geometry, TextOverflowMode.Word, _uiRenderLayer: 11), false);
            //layout.AddToLayout(new Panel(Color.Black), false);
            layout.AddToLayout(new Panel(Color.Red), false);
            layout.AddToLayout(new Panel(Color.Orange));
        }

        protected override void Update()
        {
            CameraMovement();
        }

        private void CameraMovement()
        {
            Camera2D.main.zoom += Input.scrollDelta;
            Camera2D.main.zoom = Math.Clamp(Camera2D.main.zoom, 0.5f, 2);

            float moveSpeed = 1000;
            if (Input.GetKey(Keys.W))
            {
                Camera2D.main.position += new Vector2(0, moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.A))
            {
                Camera2D.main.position += new Vector2(-moveSpeed * Time.DeltaTime, 0);
            }
            if (Input.GetKey(Keys.S))
            {
                Camera2D.main.position += new Vector2(0, -moveSpeed * Time.DeltaTime);
            }
            if (Input.GetKey(Keys.D))
            {
                Camera2D.main.position += new Vector2(moveSpeed * Time.DeltaTime, 0);
            }
        }

        protected unsafe override void Render()
        {
            frames++;
            if(Time.TotalElapsedSeconds - lastFrameCountTime >= 1)
            {
                lastFrameCountTime = Time.TotalElapsedSeconds;
                displayFrames = frames;
                frames = 0;
            }

            //fpsLabel.Text = $"FPS: {displayFrames}";
        }
    }
}
