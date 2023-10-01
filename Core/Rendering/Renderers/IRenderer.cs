﻿using Electron2D.Core.Rendering.Shaders;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// An interface that allows an object to be used as a renderer and be passed into GameObjects.
    /// </summary>
    public interface IRenderer
    {
        public void Load();
        public void Render();
        public void SetVertexValueAll(int _type, float _value);
        public float GetVertexValue(int _type, int _vertex = 0);
        public void SetMaterial(Material _material);
        public Material GetMaterial();

        /// <summary>
        /// This is true if the vertex data has been updated this frame
        /// </summary>
        public bool IsDirty { get; set; }
        public bool IsLoaded { get; set; }
    }
}
