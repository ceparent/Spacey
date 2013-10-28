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
    public enum GameState { Running, Menu }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public Player Player;
        public GameState State { get; set; }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = false;

            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1800;
            State = GameState.Running;

        }
        
        protected override void Initialize()
        {
            InitializeComponents();
  

            base.Initialize();
        }


        // TODO: ADD zones!!!

        CameraGC _CameraComponent;
        TerrainGC _TerrainComponent;
        private void InitializeComponents()
        {
            _CameraComponent = new CameraGC(this);
            _TerrainComponent = new TerrainGC(this);

            //Update Order
            int up = 0;
            _CameraComponent.UpdateOrder = up++;
            _TerrainComponent.UpdateOrder = up++;

            //Draw order
            int draw = 0;
            _TerrainComponent.DrawOrder = draw++;


            Components.Add(_CameraComponent);
            Components.Add(_TerrainComponent);
        }

        Texture2D _crosshairTexture;
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            _devFont = Content.Load<SpriteFont>("DevFont");
            _crosshairTexture = Content.Load<Texture2D>("HUD/crosshair");


            Player = new Player(this);

        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            Player.Update();


            base.Update(gameTime);
            
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullCounterClockwiseFace;
            rs.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rs;

            DepthStencilState depthBufferState = new DepthStencilState();
            depthBufferState.DepthBufferEnable = true;
            
            GraphicsDevice.DepthStencilState = depthBufferState;


            base.Draw(gameTime);

            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            DrawInfos(frameRate);
            DrawHUD();

        }


        SpriteFont _devFont;
        private float _lowestFps = float.MaxValue;
        private void DrawInfos(float pFps)
        {
            spriteBatch.Begin();
            float offset = 25;
            int cpt = 0;

            //Player
            Vector3 RotatedTarget = _CameraComponent.RotatedTarget;
            int shaderIndex = _TerrainComponent.ShaderIndex;

            spriteBatch.DrawString(_devFont, "Position { X : " + Player.HeadPosition.X.ToString("n2") + " // Y : " + Player.feetPosition.Y.ToString("n2") + "(+5) // Z: " + Player.HeadPosition.Z.ToString("n2") + " } ", new Vector2(0, offset * cpt++), Color.White);
            spriteBatch.DrawString(_devFont, "Face {  X : " + RotatedTarget.X.ToString("n2") + "  // Y : " + RotatedTarget.Y.ToString("n2") + " // Z : " + RotatedTarget.Z.ToString("n2") + " } ", new Vector2(0, offset * cpt++), Color.White);
            cpt++;
            spriteBatch.DrawString(_devFont, "ShaderIndex : " + shaderIndex, new Vector2(0, offset * cpt++), Color.White);
            spriteBatch.DrawString(_devFont, "Gravity : " + Player._Gravity.Y, new Vector2(0, offset * cpt++), Color.White);
            spriteBatch.DrawString(_devFont, "Velocity : " + Player._velocity.Y, new Vector2(0, offset * cpt++), Color.White);


            //FPS:
            if (_lowestFps > pFps)
                _lowestFps = pFps;
            spriteBatch.DrawString(_devFont, "FPS : " + pFps.ToString("n0"), new Vector2(Window.ClientBounds.Width - _devFont.MeasureString("FPS : " + pFps.ToString("n0")).X, 0), Color.Yellow);
           // spriteBatch.DrawString(_devFont, "LOW : " + _lowestFps.ToString("n0"), new Vector2(Window.ClientBounds.Width - _devFont.MeasureString("LOW : " + _lowestFps.ToString("n0")).X, offset), Color.Yellow);

            spriteBatch.End();
        }

        private void DrawHUD()
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_crosshairTexture, new Vector2(Window.ClientBounds.Width / 2 - _crosshairTexture.Width/2, Window.ClientBounds.Height / 2 - _crosshairTexture.Height /2), Color.DarkRed);

            spriteBatch.End();
        }

    }
}
