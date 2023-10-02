﻿using Electron2D.Core.Rendering;
using System.Drawing;
using System.Drawing.Imaging;
using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Management
{
    public static class TextureFactory
    {
        public static Texture2D Load(string _textureName)
        {
            uint handle = glGenTexture();
            int textureUnit = GL_TEXTURE0;
            glActiveTexture(textureUnit);
            glBindTexture(GL_TEXTURE_2D, handle);
            using var image = new Bitmap(_textureName);
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            var data = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
            glGenerateMipmap(GL_TEXTURE_2D);
            return new Texture2D(handle, image.Width, image.Height);
        }
    }
}
