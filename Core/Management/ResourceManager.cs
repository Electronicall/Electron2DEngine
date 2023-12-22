﻿using Electron2D.Core.Audio;
using Electron2D.Core.Management.Textures;
using Electron2D.Core.Rendering;
using Electron2D.Core.Rendering.Text;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Electron2D.Core.Management
{
    public sealed class ResourceManager
    {
        private static ResourceManager instance = null;
        private static readonly object loc = new();
        private IDictionary<string, Texture2DArray> textureArrayCache = new Dictionary<string, Texture2DArray>();
        private IDictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private IDictionary<uint, Texture2D> textureHandleCache = new Dictionary<uint, Texture2D>();
        private IDictionary<FontArguments, FontGlyphStore> fontCache = new Dictionary<FontArguments, FontGlyphStore>();
        private IDictionary<string, CachedSound> soundCache = new Dictionary<string, CachedSound>();

        public static ResourceManager Instance
        {
            get
            {
                lock(loc)
                {
                    if(instance == null)
                    {
                        instance = new ResourceManager();
                    }
                    return instance;
                }
            }
        }

        #region Texture Arrays
        /// <summary>
        /// Removes a texture from the cache. This is called from <see cref="Texture2DArray.Dispose()"/>
        /// </summary>
        /// <param name="_texture">The texture to remove from the cache.</param>
        public void RemoveTextureArray(Texture2DArray _texture)
        {
            foreach (var pair in textureArrayCache)
            {
                if (pair.Value == _texture)
                {
                    textureCache.Remove(pair.Key);
                }
            }
        }

        /// <summary>
        /// Directly loads a vertical spritesheet as a texture array, from bottom to top.
        /// </summary>
        /// <param name="_textureFileName"></param>
        /// <returns></returns>
        public Texture2DArray LoadTextureArray(string _textureFileName, int _layers, bool _loadAsNonSRGBA = false)
        {
            textureArrayCache.TryGetValue(_textureFileName, out var value);
            if (value is not null)
            {
                return value;
            }

            value = TextureFactory.LoadArray(_textureFileName, _layers, _loadAsNonSRGBA);
            textureArrayCache.Add(_textureFileName, value);
            return value;
        }

        /// <summary>
        /// Loads multiple individual textures into one texture array. All textures must be the same size.
        /// </summary>
        /// <param name="_textureFileNames"></param>
        /// <param name="_loadAsNonSRGBA"></param>
        /// <returns></returns>
        public Texture2DArray LoadTextureArray(string[] _textureFileNames, bool _loadAsNonSRGBA = false)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _textureFileNames.Length; i++)
            {
                builder.Append(_textureFileNames[i]);
            }

            textureArrayCache.TryGetValue(builder.ToString(), out var value);
            if (value is not null)
            {
                return value;
            }

            value = TextureFactory.LoadArray(_textureFileNames, _loadAsNonSRGBA);
            textureArrayCache.Add(builder.ToString(), value);
            return value;
        }

        /// <summary>
        /// Loads a spritesheet as a texture array, left to right, top to bottom.
        /// </summary>
        /// <param name="_textureFileName"></param>
        /// <param name="_spriteWidth"></param>
        /// <param name="_spriteHeight"></param>
        /// <param name="_loadAsNonSRGBA"></param>
        /// <returns></returns>
        public Texture2DArray LoadTextureArray(string _textureFileName, int _spriteWidth,  int _spriteHeight, bool _loadAsNonSRGBA = false)
        {
            textureArrayCache.TryGetValue(_textureFileName, out var value);
            if (value is not null)
            {
                return value;
            }

            value = TextureFactory.LoadArray(_textureFileName, _spriteWidth, _spriteHeight, _loadAsNonSRGBA);
            textureArrayCache.Add(_textureFileName, value);
            return value;
        }
        #endregion

        #region Textures
        /// <summary>
        /// Loads a texture into memory and returns it.
        /// After a texture is loaded, the already stored texture will be returned instead of creating a new Texture2D object.
        /// </summary>
        /// <param name="_textureFileName">The local file path of the texture. Ex. Build/Resources/Textures/TextureNameHere.png</param>
        /// <returns></returns>
        public Texture2D LoadTexture(string _textureFileName, bool _loadAsNonSRGBA = false)
        {
            textureCache.TryGetValue(_textureFileName, out var value);
            if(value is not null)
            {
                return value;
            }

            value = TextureFactory.Load(_textureFileName, _loadAsNonSRGBA);
            textureCache.Add(_textureFileName, value);
            textureHandleCache.Add(value.Handle, value);
            return value;
        }

        /// <summary>
        /// Returns a previously loaded texture using it's OpenGL handle.
        /// </summary>
        /// <param name="_handle"></param>
        /// <returns></returns>
        public Texture2D GetTexture(uint _handle)
        {
            textureHandleCache.TryGetValue(_handle, out var value);
            if (value is not null)
            {
                return value;
            }

            Debug.LogError($"Texture with handle {_handle} does not exist.");
            return null;
        }

        /// <summary>
        /// Removes a texture from the cache. This is called from <see cref="Texture2D.Dispose()"/>
        /// </summary>
        /// <param name="_texture">The texture to remove from the cache.</param>
        public void RemoveTexture(Texture2D _texture)
        {
            textureHandleCache.Remove(_texture.Handle);
            foreach (var pair in textureCache)
            {
                if(pair.Value == _texture)
                {
                    textureCache.Remove(pair.Key);
                }
            }
        }

        /// <summary>
        /// Creates a blank texture object.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public object CreateTexture(int _width, int _height)
        {
            Texture2D texture = TextureFactory.Create(_width, _height);
            textureHandleCache.Add(texture.Handle, texture);
            return texture.Handle;
        }

        /// <summary>
        /// Returns the size of a texture. Used by <see cref="ITexture2DManager"/>.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public Point GetTextureSize(object _texture)
        {
            if(textureHandleCache.TryGetValue((uint)_texture, out Texture2D tex))
            {
                return new Point(tex.Width, tex.Height);
            }
            else
            {
                Debug.LogError($"Texture handle {_texture} is not registered, cannot get size.");
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Sets the pixel data of a texture.
        /// </summary>
        /// <param name="_texture"></param>
        /// <param name="_bounds"></param>
        /// <param name="_data"></param>
        public void SetTextureData(object _texture, Rectangle _bounds, byte[] _data)
        {
            if (textureHandleCache.TryGetValue((uint)_texture, out Texture2D tex))
            {
                tex.SetData(_bounds, _data);
            }
            else
            {
                Debug.LogError($"Texture handle {_texture} is not registered, cannot set data.");
            }
        }
        #endregion

        #region Fonts
        public FontGlyphStore LoadFont(string _fontFile, int _fontSize, int _outlineSize)
        {
            string[] s = _fontFile.Split('/');
            FontArguments args = new FontArguments() { FontName = s[s.Length - 1], FontSize = _fontSize };
            fontCache.TryGetValue(args, out var value);
            if (value is not null)
            {
                return value;
            }

            value = FontGlyphFactory.Load(_fontFile, _fontSize, _outlineSize);
            fontCache.Add(args, value);
            return value;
        }
        #endregion

        #region Audio
        /// <summary>
        /// Loads a sound into memory and returns it.
        /// After a sound is loaded, the already stored sound will be returned instead of creating a new CachedSound object.
        /// </summary>
        /// <param name="_soundFileName">The local file path of the sound. Ex. Build/Resources/Audio/SFX/SoundFileNameHere.mp3</param>
        /// <returns></returns>
        public CachedSound LoadSound(string _soundFileName)
        {
            soundCache.TryGetValue(_soundFileName, out var value);
            if (value is not null)
            {
                return value;
            }

            value = new CachedSound(_soundFileName);
            soundCache.Add(_soundFileName, value);
            return value;
        }
        #endregion
    }
}
