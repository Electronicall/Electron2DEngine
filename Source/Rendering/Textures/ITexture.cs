﻿using System.Numerics;

namespace Electron2D.Rendering
{
    public interface ITexture
    {
        public abstract void Use(int _textureSlot);
        public abstract void SetFilteringMode(bool _linear);
        public virtual int GetTextureLayers() { return -1; }
        public virtual uint GetHandle() { return 0; }
        public Vector2 GetSize();
    }
}
