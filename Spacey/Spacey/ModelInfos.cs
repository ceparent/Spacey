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
using System.IO;
namespace Spacey
{
    class ModelInfos
    {
        public Vector3 Position { get; set; }
        public float Scale { get; set; }

        public ModelInfos(Vector3 pPosition, float pScale)
        {
            Scale = pScale;
            Position = pPosition;
        }
    }
}
