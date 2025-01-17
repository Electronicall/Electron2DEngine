﻿using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Rendering
{
    public class Texture2D : ITexture, IDisposable
    {
        private bool _disposed;
        public uint Handle { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FilePath { get; }
        public bool IsSRGBA { get; }

        public Texture2D(uint _handle, int _width, int _height, string _filePath = "", bool _isSRGBA = true)
        {
            Handle = _handle;
            Width = _width;
            Height = _height;
            FilePath = _filePath;
            IsSRGBA = _isSRGBA;
        }

        ~Texture2D()
        {
            Dispose(false);
        }

        public uint GetHandle() => Handle;

        public Vector2 GetSize() => new Vector2(Width, Height);

        public void Use(int _textureSlot)
        {
            glActiveTexture(_textureSlot);
            glBindTexture(GL_TEXTURE_2D, Handle);
        }

        public unsafe void SetData(Rectangle _bounds, byte[] _data)
        {
            // Using not in-use texture slot since this does not cache itself as the last used texture anywhere
            Use(GL_TEXTURE0);
            fixed (byte* ptr = _data)
            {
                glTexSubImage2D(GL_TEXTURE_2D, 0, _bounds.Left, _bounds.Top, _bounds.Width, _bounds.Height, GL_RGBA, GL_UNSIGNED_BYTE, ptr);
            }
        }

        public unsafe Bitmap GetData(int format = GL_BGRA)
        {
            Use(GL_TEXTURE0);
            Bitmap bitmap = new Bitmap(Width, Height);
            var data = bitmap.LockBits(
                        new Rectangle(0, 0, Width, Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

            glGetTexImage(GL_TEXTURE_2D, 0, format, GL_UNSIGNED_BYTE, data.Scan0);

            bitmap.UnlockBits(data);
            return bitmap;
        }

        public unsafe void Save(string _filePath)
        {
            Use(GL_TEXTURE0);
            Bitmap bitmap = new Bitmap(Width, Height);
            var data = bitmap.LockBits(
                        new Rectangle(0, 0, Width, Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

            glGetTexImage(GL_TEXTURE_2D, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

            bitmap.UnlockBits(data);
            bitmap.Save(_filePath, ImageFormat.Png);
            bitmap.Dispose();
        }

        public void SetFilteringMode(bool _linear)
        {
            Use(GL_TEXTURE0);

            if (_linear)
            {
                // Linear filtering
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            }
            else
            {
                // Nearest filtering
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            }
        }

        private void Dispose(bool _disposing)
        {
            if(!_disposed)
            {
                glDeleteTexture(Handle);
                ResourceManager.Instance.RemoveTexture(this);
                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
