﻿using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using System.Drawing;

namespace Electron2D.Core.UserInterface
{
    /// <summary>
    /// The most basic UI component.
    /// </summary>
    public class Panel : UiComponent
    {
        public float[] vertices { get; private set; } = new float[16];
        public static readonly uint[] indices =
        {
            3, 1, 0, // Triangle 1
            3, 2, 1  // Triangle 2
        };

        private List<float> tempVertexList = new List<float>();

        public Panel(Color _mainColor, int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100,
            bool _useScreenPosition = true) : base(_uiRenderLayer, _sizeX, _sizeY, _useScreenPosition: _useScreenPosition)
        {
            GenerateVertices();
            meshRenderer.SetVertexArrays(vertices, indices);
            SetColor(_mainColor);
        }
        public Panel(Material _material, int _uiRenderLayer = 0, int _sizeX = 100, int _sizeY = 100,
            bool _useScreenPosition = true) : base(_uiRenderLayer, _sizeX, _sizeY, _useScreenPosition: _useScreenPosition)
        {
            GenerateVertices();
            meshRenderer.SetVertexArrays(vertices, indices);
            meshRenderer.SetMaterial(_material);
        }

        private void GenerateVertices()
        {
            tempVertexList.Clear();

            // Top Right
            tempVertexList.Add(RightXBound * 2);
            tempVertexList.Add(TopYBound * 2);
            tempVertexList.Add(1);
            tempVertexList.Add(1);

            // Bottom Right
            tempVertexList.Add(RightXBound * 2);
            tempVertexList.Add(BottomYBound * 2);
            tempVertexList.Add(1);
            tempVertexList.Add(0);

            // Bottom Left
            tempVertexList.Add(LeftXBound * 2);
            tempVertexList.Add(BottomYBound * 2);
            tempVertexList.Add(0);
            tempVertexList.Add(0);

            // Top Left
            tempVertexList.Add(LeftXBound * 2);
            tempVertexList.Add(TopYBound * 2);
            tempVertexList.Add(0);
            tempVertexList.Add(1);

            for (int i = 0; i < tempVertexList.Count; i++)
            {
                vertices[i] = tempVertexList[i];
            }
        }


        public override void UpdateMesh()
        {
            GenerateVertices();
            meshRenderer.IsDirty = true;
        }
    }
}
