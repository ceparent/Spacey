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

    class TerrainGC:DrawableGameComponent,ITerrainComponent
    {
        public TerrainGC(Game pGame)
            : base(pGame)
        {
            Game.Services.AddService(typeof(ITerrainComponent), this);
        }
        public override void Initialize()
        {
            base.Initialize();
        }

        public BoundingBox[] BoundingBoxes
        {
            get
            {
                List<BoundingBox> boxes = new List<BoundingBox>();
                foreach (DrawableBox box in _boxes)
                {
                    boxes.Add(box.BoundingBox);
                }
                return boxes.ToArray();
            }
        }

        private int _shaderIndex;
        public int ShaderIndex
        {
            get
            {
                return _shaderIndex;
            }
            set
            {
                _shaderIndex = value;
            }
        }


        private BasicEffect _Basiceffect;
        private Dictionary<string, Effect> _effects;

        private GraphicsDevice _device;
        private Dictionary<string, Texture2D> _textures;
        protected override void LoadContent()
        {
            _device = Game.GraphicsDevice;
            _Basiceffect = new BasicEffect(_device);

            _effects = new Dictionary<string, Effect>();
            _effects.Add("Ambient",Game.Content.Load<Effect>("Shaders/Ambient"));
            _effects.Add("Diffuse",Game.Content.Load<Effect>("Shaders/Diffuse"));
            _effects.Add("Specular", Game.Content.Load<Effect>("Shaders/Specular"));
            _effects.Add("Textured", Game.Content.Load<Effect>("Shaders/Textured"));
            _effects.Add("Test", Game.Content.Load<Effect>("Shaders/Test"));

            _textures = new Dictionary<string, Texture2D>();
            _textures.Add("floor_concrete",Game.Content.Load<Texture2D>("Textures/floor_concrete"));
            _textures.Add("wall_brick", Game.Content.Load<Texture2D>("Textures/wall_brick"));
            _textures.Add("arrow", Game.Content.Load<Texture2D>("Textures/arrow"));

            LoadMap();
            LoadModels();
            LoadDecals();


            CreateBuffers();
            base.LoadContent();
        }

        Dictionary<string, Model> _models;
        private void LoadModels()
        {
            //TODO: Allow multiple instances of the same model

            Vector3 position;
            Model model;
            string modelName;

            modelName = "xWing";
            _models = new Dictionary<string, Model>();
            model = Game.Content.Load<Model>(modelName);
            _models.Add(modelName, model);
            position = new Vector3(20, 5, -50);
            model.Tag = new ModelInfos(position, 0.05f);


            modelName = "monkey";
            model = Game.Content.Load<Model>(modelName);
            _models.Add(modelName, model);
            position = new Vector3(45, 5, -50);
            model.Tag = new ModelInfos(position, 1f);

        }

        List<Decal> _decals;
        private void LoadDecals()
        {
            _decals = new List<Decal>();
            //Decal decal;

            //decal = new Decal(new Vector3(0,20,0), Vector3.Down, Vector3.Left, _textures["arrow"]);
            //_decals.Add(decal);

        }


        List<DrawableBox> _boxes;
        private void LoadMap()
        {
            _boxes = new List<DrawableBox>();

            // around map
            //_boxes.Add(new DrawableBox(new Vector3(-80, 10, -100), new Vector3(-80, -5, 0), _textures["wall_brick"]));
            TextureInfos wallInfos = new TextureInfos(_textures["wall_brick"], Vector2.Zero, true, 20f);

            //Building
            _boxes.Add(new DrawableBox(new Vector3(-40, 12, -20), new Vector3(-10, 21, 0), wallInfos));

            _boxes.Add(new DrawableBox(new Vector3(-20, 0, -20), new Vector3(-10, 8f, 0), wallInfos));
            _boxes.Add(new DrawableBox(new Vector3(-10, 0, -20), new Vector3(0, 6f, 0), wallInfos));
           
            _boxes.Add(new DrawableBox(new Vector3(0, 0, -20), new Vector3(10, 4f, 0), wallInfos));
            _boxes.Add(new DrawableBox(new Vector3(10, 0, -20), new Vector3(20, 2f, 0), wallInfos));




            TextureInfos floorInfos = new TextureInfos(_textures["floor_concrete"], Vector2.Zero, true, 20f);
            //base
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    _boxes.Add(new DrawableBox(new Vector3(20 * i, -20, 20 * j), new Vector3(20 * i + 20, 0, 20 * j + 20), floorInfos));
                }
            }

            _boxes.Add(new DrawableBox(new Vector3(-20, 8f, -20), new Vector3(-10, 8.01f, 0), floorInfos));
            _boxes.Add(new DrawableBox(new Vector3(-10, 6f, -20), new Vector3(0, 6.01f, 0), floorInfos));
            _boxes.Add(new DrawableBox(new Vector3(0, 4f, -20), new Vector3(10, 4.01f, 0), floorInfos));
            _boxes.Add(new DrawableBox(new Vector3(10, 2f, -20), new Vector3(20, 2.01f, 0), floorInfos));
            _boxes.Add(new DrawableBox(new Vector3(-40, 21, -20), new Vector3(-20, 21.01f, 0), floorInfos));
        }

        public CollisionInfos CollideBox(BoundingBox pBox)
        {
            List<BoundingBox> boxes = new List<BoundingBox>();
            foreach (DrawableBox box in _boxes)
            {
                if (box.BoundingBox.Contains(pBox) == ContainmentType.Contains)
                {
                    return new CollisionInfos(new BoundingBox[] { box.BoundingBox }, ContainmentType.Contains, CollisionHandling(pBox, box.BoundingBox));
                }
                if (box.BoundingBox.Contains(pBox) != ContainmentType.Disjoint)
                {
                    boxes.Add(box.BoundingBox);
                }
            }
            return new CollisionInfos(boxes.ToArray(), ContainmentType.Intersects, null);
        }

        private Vector3 CollisionHandling(BoundingBox playerBox, BoundingBox collisionBox)
        {
            float tolerance = 1f;
            Vector3 point = playerBox.Min;

            float diffMinX = point.X - collisionBox.Min.X;
            float diffMaxX = collisionBox.Max.X - point.X;
            float diffMinZ = point.Z - collisionBox.Min.Z;
            float diffMaxZ = collisionBox.Max.Z - point.Z;

            if (diffMinX < tolerance)
                point.X -= diffMinX;
            if (diffMinZ < tolerance)
                point.Z -= diffMinZ;
            if (diffMaxX < tolerance)
                point.X += diffMaxX;
            if (diffMaxZ < tolerance)
                point.Z += diffMaxZ;

            return point;
        }





        private void CreateBuffers()
        {
            foreach (DrawableBox box in _boxes)
            {
                box.CreateVBuffer(_device);
            }

            foreach (Decal decal in _decals)
            {
                decal.CreateBuffers(_device);
            }

        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            SamplerState ss = new SamplerState();
            ss.AddressU = TextureAddressMode.Wrap;
            ss.AddressV = TextureAddressMode.Wrap;

            GraphicsDevice.SamplerStates[0] = ss;

            if (ShaderIndex == 0)
            {
                ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));

                _Basiceffect.EnableDefaultLighting();
                _Basiceffect.SpecularPower = 1000f;
                _Basiceffect.SpecularColor = Color.Gray.ToVector3();


                _Basiceffect.World = Matrix.Identity;
                _Basiceffect.View = camera.ViewMatrix;
                _Basiceffect.Projection = camera.ProjectionMatrix;
                _Basiceffect.TextureEnabled = true;
                _Basiceffect.FogEnabled = true;
                _Basiceffect.FogColor = Color.Black.ToVector3();
                _Basiceffect.FogStart = 60f;
                _Basiceffect.FogEnd = 170f;

            }




            KeyValuePair<string,Model>[] modelArray = _models.ToArray();
            foreach (KeyValuePair<string, Model> model in modelArray)
            {
                
                switch (ShaderIndex)
                {
                    case 0:
                        DrawModelBasicEffect(model.Value);
                        break;
                    case 1:
                        DrawModelsTest(model.Value);
                        break;

                }
            }

            foreach (DrawableBox box in _boxes)
            {

                switch (ShaderIndex)
                {
                    case 0:
                        DrawBoxBasicEffect(box);
                        break;
                    case 1:
                        DrawBoxTest(box);
                        break;

                }
            }


            foreach (Decal decal in _decals)
            {
                switch (ShaderIndex)
                {
                    case 0:
                        DrawDecalsBasicEffect(decal);
                        break;
                    case 1:
                        DrawDecalsTest(decal);
                        break;
                }

                
            }



            base.Draw(gameTime);
        }

        private void DrawModelBasicEffect(Model pModel)
        {
            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));


            ModelInfos infos = (ModelInfos)(pModel.Tag);
            Matrix worldMatrix =Matrix.CreateScale(infos.Scale) * Matrix.CreateTranslation(infos.Position);

            Matrix[] transforms = new Matrix[pModel.Bones.Count];
            pModel.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in pModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }
                mesh.Draw();
            }


        }

        private void DrawModelsTest(Model pModel)
        {

            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));

            ModelInfos infos = (ModelInfos)(pModel.Tag);
            Matrix worldMatrix = Matrix.CreateScale(infos.Scale) * Matrix.CreateTranslation(infos.Position);
            Matrix[] transforms = new Matrix[pModel.Bones.Count];

            pModel.CopyAbsoluteBoneTransformsTo(transforms);

            _effects["Test"].CurrentTechnique = _effects["Test"].Techniques["Test"];
            
            _effects["Test"].Parameters["View"].SetValue(camera.ViewMatrix);
            _effects["Test"].Parameters["Projection"].SetValue(camera.ProjectionMatrix);

            Queue<Effect> effects = new Queue<Effect>();
            foreach (ModelMesh mesh in pModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    effects.Enqueue(part.Effect);
                    part.Effect = _effects["Test"];
                    _effects["Test"].Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * worldMatrix);
                }
                mesh.Draw();
            }

            foreach (ModelMesh mesh in pModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effects.Dequeue();
                }
            }



        }



        private void DrawBoxBasicEffect(DrawableBox pBox)
        {
            _Basiceffect.Texture = pBox.Texture;

            foreach (EffectPass pass in _Basiceffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.SetVertexBuffer(pBox.VBuffer);
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, DrawableBox.NB_PRIMITVES);


            }

        }


        private void DrawDecalsBasicEffect(Decal pDecal)
        {

            _Basiceffect.World = pDecal.WorldMatrix;
            _Basiceffect.Texture = pDecal.Texture;

            foreach (EffectPass pass in _Basiceffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _device.Indices = pDecal.IndexBuffer;
                _device.SetVertexBuffer(pDecal.VertexBuffer);
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, pDecal.VertexBuffer.VertexCount,0 ,  pDecal.IndexBuffer.IndexCount / 3);

            }

        }

        private void DrawDecalsTest(Decal pDecal)
        {

            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));
            _effects["Test"].CurrentTechnique = _effects["Test"].Techniques["Test"];
            _effects["Test"].Parameters["World"].SetValue(pDecal.WorldMatrix);
            _effects["Test"].Parameters["View"].SetValue(camera.ViewMatrix);
            _effects["Test"].Parameters["Projection"].SetValue(camera.ProjectionMatrix);



            foreach (EffectPass pass in _effects["Test"].CurrentTechnique.Passes)
            {
                pass.Apply();

                _device.Indices = pDecal.IndexBuffer;
                _device.SetVertexBuffer(pDecal.VertexBuffer);
                _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, pDecal.VertexBuffer.VertexCount, 0, pDecal.IndexBuffer.IndexCount / 3);

            }

        }


        private void DrawBoxTest(DrawableBox pBox)
        {

            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));
            _effects["Test"].CurrentTechnique = _effects["Test"].Techniques["Test"];
            _effects["Test"].Parameters["World"].SetValue(Matrix.Identity);
            _effects["Test"].Parameters["View"].SetValue(camera.ViewMatrix);
            _effects["Test"].Parameters["Projection"].SetValue(camera.ProjectionMatrix);



            foreach (EffectPass pass in _effects["Test"].CurrentTechnique.Passes)
            {
                pass.Apply();
                _device.SetVertexBuffer(pBox.VBuffer);
                _device.DrawPrimitives(PrimitiveType.TriangleList, 0, DrawableBox.NB_PRIMITVES);

            }
        }

        public void AddDecal(Decal pDecal)
        {
            pDecal.CreateBuffers(Game.GraphicsDevice);
            _decals.Add(pDecal);
        }


    }

    public class CollisionInfos
    {
        public BoundingBox[] CollisionBoxes { get; set; }
        public ContainmentType ContainmentType { get; set; }
        public Vector3? DeepSide { get; set; }

        public CollisionInfos(BoundingBox[] boxes, ContainmentType type, Vector3? deepSide)
        {
            ContainmentType = type;
            CollisionBoxes = boxes;
            DeepSide = deepSide;

        }

    }
}
