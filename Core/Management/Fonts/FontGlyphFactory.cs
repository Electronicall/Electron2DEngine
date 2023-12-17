﻿using Electron2D.Core.Rendering.Text;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using System.Drawing;
using System.Numerics;
using static Electron2D.OpenGL.GL;
using static FreeTypeSharp.Native.FT;

namespace Electron2D.Core.Management
{
    public static class FontGlyphFactory
    {
        public static FontGlyphStore Load(string _fontFile, int _fontSize, int _outlineWidth)
        {
            FreeTypeLibrary library = new FreeTypeLibrary();
            IntPtr face;
            FT_Error error;

            error = FT_New_Face(library.Native, _fontFile, 0, out face);
            if (error == FT_Error.FT_Err_Unknown_File_Format)
            {
                Debug.LogError($"FREETYPE: The font file {_fontFile} could be opened and read, but it appears that its font format is unsupported");
                return null;
            }
            else if (error != FT_Error.FT_Err_Ok)
            {
                Debug.LogError($"FREETYPE: The font file {_fontFile} could not be opened or read");
                return null;
            }

            // Setting font pixel size, 0 width means dynamically scaled with height
            FT_Set_Pixel_Sizes(face, 0, (uint)_fontSize);

            FreeTypeFaceFacade f = new FreeTypeFaceFacade(library, face);
            FontGlyphStore store = new FontGlyphStore(_fontSize, _fontFile, library, face, f.HasKerningFlag);

            //// Stroker is used for outline
            //IntPtr stroker = IntPtr.Zero;
            //if (_outlineWidth > 0)
            //{
            //    FT_Stroker_New(library.Native, out stroker);
            //    FT_Stroker_Set(stroker, _outlineWidth, FT_Stroker_LineCap.FT_STROKER_LINECAP_ROUND, FT_Stroker_LineJoin.FT_STROKER_LINEJOIN_ROUND, IntPtr.Zero);
            //}

            glPixelStorei(GL_UNPACK_ALIGNMENT, 1);
            for (uint c = 0; c < 128; c++)
            {
                if (FT_Load_Char(face, c, FT_LOAD_RENDER) != FT_Error.FT_Err_Ok)
                {
                    Debug.LogError($"FREETYPE: Failed to load glyph '{(char)c}'");
                    continue;
                }

                uint texture = glGenTexture();
                glActiveTexture(GL_TEXTURE10);
                glBindTexture(GL_TEXTURE_2D, texture);
                glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, (int)f.GlyphBitmap.width, (int)f.GlyphBitmap.rows, 0, GL_RED, GL_UNSIGNED_BYTE, f.GlyphBitmap.buffer);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_BORDER);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_BORDER);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

                Character character = new Character(texture, new Vector2((int)f.GlyphBitmap.width, (int)f.GlyphBitmap.rows),
                    new Vector2(f.GlyphBitmapLeft, f.GlyphBitmapTop), (uint)f.GlyphMetricHorizontalAdvance);
                store.AddCharacter((char)c, character);
            }
            glPixelStorei(GL_UNPACK_ALIGNMENT, 4);

            return store;
        }
    }
}
