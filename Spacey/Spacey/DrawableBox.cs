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
    class DrawableBox
    {
        public static int NB_PRIMITVES = 12;
        private BoundingBox ViewableBoundingBox { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public bool TextureRepeat { get; set; }

        public Texture2D Texture { get; set; }
        public VertexBuffer VBuffer { get; private set; }
        private Vector2 Offset;
        private float RepeatLength;

        public DrawableBox(Vector3 v1, Vector3 v2, TextureInfos pInfos)
        {
            Vector3 min = v1;
            Vector3 max = v2;

            findMinMax(ref min,ref max);

            TextureRepeat = pInfos.Repeat;
            RepeatLength = pInfos.RepeatLength;
            Offset = pInfos.Offset;
            

            ViewableBoundingBox= new BoundingBox(min, max);
            BoundingBox = ViewableBoundingBox;
            Texture = pInfos.Texture;

            _vertices = getVertices();
        }

        private void findMinMax(ref Vector3 min, ref Vector3 max)
        {
            Vector3 diff = max - min;

            int cpt = 0;

            if (diff.X < 0)
            {
                float temp = max.X;
                max.X = min.X;
                min.X = temp;
            }
            if (diff.Y < 0)
            {
                float temp = max.Y;
                max.Y = min.Y;
                min.Y = temp;
            }
            if (diff.Z < 0)
            {
                float temp = max.Z;
                max.Z = min.Z;
                min.Z = temp;
            }

            if (cpt == 1 || cpt == 3)
            {

            }
        }


        public void CreateVBuffer(GraphicsDevice device)
        {
            VBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), 36, BufferUsage.WriteOnly);
            VBuffer.SetData(_vertices);
        }


        private VertexPositionNormalTexture[] _vertices;
        private VertexPositionNormalTexture[] getVertices()
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[36];

            Vector3[] corners = ViewableBoundingBox.GetCorners();

            // 7 3 6 2 4 0 5 1

            Vector3 frontTopLeft = corners[3];
            Vector3 frontTopRight = corners[2];
            Vector3 frontBottomLeft = corners[7];
            Vector3 frontBottomRight = corners[6];

            Vector3 backTopLeft = corners[0];
            Vector3 backTopRight = corners[1];
            Vector3 backBottomLeft = corners[4];
            Vector3 backBottomRight = corners[5];


            Vector2 topLeft = new Vector2(0, 0);
            Vector2 topRight = new Vector2(1, 0);
            Vector2 botLeft = new Vector2(0, 1);
            Vector2 botRight = new Vector2(1, 1);

            //TODO: implement offsets for texture

            int cpt = 0;
            Vector3 normal = Vector3.Zero;

            // Face Bottom

            normal = Vector3.Down;

            float tl = RepeatLength;

            topLeft = new Vector2(frontTopLeft.X / tl, frontTopLeft.Z / tl);
            topRight = new Vector2(frontTopRight.X / tl, frontTopRight.Z / tl);
            botLeft = new Vector2(frontBottomLeft.X / tl, frontBottomLeft.Z / tl);
            botRight = new Vector2(frontBottomRight.X / tl, frontBottomRight.Z / tl);


            vertices[cpt++] = new VertexPositionNormalTexture(frontTopLeft, normal, topLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontTopRight, normal, topRight);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomRight, normal, botRight);

            vertices[cpt++] = new VertexPositionNormalTexture(frontTopLeft, normal, topLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomRight, normal, botRight);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomLeft, normal, botLeft);


            //Face Top

            topLeft = new Vector2(backTopRight.X / tl, backTopRight.Z / tl);
            topRight = new Vector2(backTopLeft.X / tl, backTopLeft.Z / tl);
            botLeft = new Vector2(backBottomRight.X / tl, backBottomRight.Z / tl);
            botRight = new Vector2(backBottomLeft.X / tl, backBottomLeft.Z / tl);

            normal = Vector3.Up;
            vertices[cpt++] = new VertexPositionNormalTexture(backTopLeft, normal, topRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomRight, normal, botLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(backTopRight, normal, topLeft);

            vertices[cpt++] = new VertexPositionNormalTexture(backTopLeft, normal, topRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomLeft, normal, botRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomRight, normal, botLeft);


            // face Left


            topLeft = new Vector2(frontTopLeft.Z / tl, frontTopLeft.Y / tl);
            topRight = new Vector2(frontBottomLeft.Z / tl, frontBottomLeft.Y / tl);
            botLeft = new Vector2(backTopLeft.Z / tl, backTopLeft.Y / tl);
            botRight = new Vector2(backBottomLeft.Z / tl, backBottomLeft.Y / tl);

            normal = Vector3.Left;
            vertices[cpt++] = new VertexPositionNormalTexture(backTopLeft, normal, botLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontTopLeft, normal, topLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomLeft, normal, topRight);

            vertices[cpt++] = new VertexPositionNormalTexture(backTopLeft, normal, botLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomLeft, normal, topRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomLeft, normal, botRight);


            // Face right

            topLeft = new Vector2(frontBottomRight.Z / tl, frontBottomRight.Y / tl);
            topRight = new Vector2(frontTopRight.Z / tl, frontTopRight.Y / tl);
            botLeft = new Vector2(backBottomRight.Z / tl, backBottomRight.Y / tl);
            botRight = new Vector2(backTopRight.Z / tl, backTopRight.Y / tl);

            normal = Vector3.Right;
            vertices[cpt++] = new VertexPositionNormalTexture(backTopRight, normal, botRight);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomRight, normal, topLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontTopRight, normal, topRight);

            vertices[cpt++] = new VertexPositionNormalTexture(backTopRight, normal, botRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomRight, normal, botLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomRight, normal, topLeft);


            //Face Top

            topLeft = new Vector2(frontTopRight.X / tl, frontTopRight.Y / tl);
            topRight = new Vector2(frontTopLeft.X / tl, frontTopLeft.Y / tl);
            botLeft = new Vector2(backTopRight.X / tl, backTopRight.Y / tl);
            botRight = new Vector2(backTopLeft.X / tl, backTopLeft.Y / tl);

            normal = Vector3.Backward;

            vertices[cpt++] = new VertexPositionNormalTexture(frontTopLeft, normal, topRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backTopLeft, normal, botRight);
            vertices[cpt++] = new VertexPositionNormalTexture(frontTopRight, normal, topLeft);

            vertices[cpt++] = new VertexPositionNormalTexture(backTopRight, normal, botLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontTopRight, normal, topLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(backTopLeft, normal, botRight);

            //Face Bottom

            topLeft = new Vector2(frontBottomLeft.X / tl, frontBottomLeft.Y / tl);
            topRight = new Vector2(frontBottomRight.X / tl, frontBottomRight.Y / tl);
            botLeft = new Vector2(backBottomLeft.X / tl, backBottomLeft.Y / tl);
            botRight = new Vector2(backBottomRight.X / tl, backBottomRight.Y / tl);

            normal = Vector3.Forward;

            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomLeft, normal, topLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomRight, normal, topRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomLeft, normal, botLeft);

            vertices[cpt++] = new VertexPositionNormalTexture(backBottomRight, normal, botRight);
            vertices[cpt++] = new VertexPositionNormalTexture(backBottomLeft, normal, botLeft);
            vertices[cpt++] = new VertexPositionNormalTexture(frontBottomRight, normal, topRight);
        
            return vertices;
        }


    }
}
