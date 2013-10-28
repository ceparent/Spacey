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
    class Decal
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Vector3 UpDirection { get; set; }
        public Texture2D Texture { get; set; }

        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }

        public Matrix WorldMatrix { get; set; }

        public Decal(Vector3 pPosition, Vector3 pDirection, Vector3 pUpDirection, Texture2D pTexture)
        {
            Position = pPosition;
            Direction = pDirection;
            UpDirection = pUpDirection;
            Texture = pTexture;

            _vertices = SetUpVertices();
            _indices = SetUpIndices();
            SetUpWorldMatrix();
        }

        VertexPositionNormalTexture[] _vertices;
        private VertexPositionNormalTexture[] SetUpVertices()
        {
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[4];
            float factor = 100;


            Vector3 topLeft = new Vector3((float)Texture.Width / factor,(float)-Texture.Height / factor, 0 );
            Vector3 topright = new Vector3((float)-Texture.Width / factor, (float)-Texture.Height / factor, 0);
            Vector3 botLeft = new Vector3((float)Texture.Width / factor, (float)Texture.Height / factor, 0);
            Vector3 botRight = new Vector3((float)-Texture.Width / factor, (float)Texture.Height / factor, 0);


            vertices[0] = new VertexPositionNormalTexture(topLeft, Vector3.Up, new Vector2(1, 1));
            vertices[1] = new VertexPositionNormalTexture(topright, Vector3.Up, new Vector2(0, 1));
            vertices[2] = new VertexPositionNormalTexture(botLeft, Vector3.Up, new Vector2(1, 0));
            vertices[3] = new VertexPositionNormalTexture(botRight, Vector3.Up, new Vector2(0, 0));

            return vertices;
        }

        int[] _indices;
        private int[] SetUpIndices()
        {
            int[] indices = new int[6];

            indices[0] = 0;
            indices[1] = 2;
            indices[2] = 1;
            indices[3] = 1;
            indices[4] = 2;
            indices[5] = 3;

            return indices;
        }

        private void SetUpWorldMatrix()
        {
            WorldMatrix = Matrix.CreateWorld(Position, Direction, UpDirection);
        }


        public void CreateBuffers(GraphicsDevice device)
        {
            VertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), 4, BufferUsage.WriteOnly);
            VertexBuffer.SetData(_vertices);
            IndexBuffer = new IndexBuffer(device, typeof(int), 6, BufferUsage.WriteOnly);
            IndexBuffer.SetData(_indices);
        }
    }
}
