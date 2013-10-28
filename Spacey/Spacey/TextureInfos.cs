using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Spacey
{
    class TextureInfos
    {
        public Texture2D Texture { get; set; }
        public Vector2 Offset { get; set; }
        public bool Repeat { get; set; }
        public float RepeatLength { get; set; }

        public TextureInfos(Texture2D pTexture, Vector2 pOffset, bool pRepeat, float pRepeatLength)
        {
            Texture = pTexture;
            Offset = pOffset;
            Repeat = pRepeat;
            RepeatLength = pRepeatLength;
        }

    }
}
