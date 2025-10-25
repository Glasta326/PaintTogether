using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.Brushes;
using PaintTogether.Core;
using PaintTogether.Core.Loadsystem;
using PaintTogether.Core.LoadSystem;

namespace PaintTogether
{
    public partial class Main : Game
    {
        public Main()
        {
            instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
            Window.AllowUserResizing = false;
        }

        protected override void Initialize()
        {
            LaunchSettings.Load();


            ILoadableRegistry.Initialize();
            ILoadableRegistry.LoadAll();
            ElementLoader.InitaliseRegistry();
            ElementLoader.LoadAll();

            
            base.Initialize();
        }

        public static RenderTarget2D Canvas;
        public static RenderTarget2D logoTarget;

        public static RenderTarget2D final;
        public static Texture2D logo;

        public static SpriteFont font;



        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            ILoadableRegistry.LoadAllAssets(GraphicsDevice, Content);
            ElementLoader.LoadAssetsAll(GraphicsDevice, Content);

            logoTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            Canvas = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            final = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            logo = Content.Load<Texture2D>("Textures/proxy-image");
            font = Content.Load<SpriteFont>("Fonts/TestFont");

        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            ILoadableRegistry.UnLoadAll();
        }

        protected override void Update(GameTime gameTime)
        {
            if (ActiveBrush is null)
            {
                ActiveBrush = ILoadableRegistry.Get<TestBrush>();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                ActiveBrush = ILoadableRegistry.Get<TestBrush>();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                ActiveBrush = ILoadableRegistry.Get<Test2Brush>();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                ActiveBrush = ILoadableRegistry.Get<TestBrush3>();
            }




            Update_Inner(gameTime);

            ElementLoader.PreUpdateAll();

            UpdateBrush();

            ElementLoader.UpdateAll();

            base.Update(gameTime);
        }

        private void Update_Inner(GameTime gameTime)
        {
            MouseUtils.State = Mouse.GetState(); // We do this and just read from state when getting mouse info so we arent requesting to get the state a zillion times
            MouseUtils.MoveHistory.Add(MouseUtils.State.Position);
            MouseUtils.ScrollHistory.Add(MouseUtils.State.ScrollWheelValue); // Push the new scroll value to the scroll history so scrollDelta is accurate
            GlobalTimeWrappedHourly = (float)(gameTime.TotalGameTime.TotalSeconds % 3600.0);
        }

        private void UpdateBrush()
        {
            if (ActiveBrush is not null)
            {
                ActiveBrush.MainUpdate();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetRenderTarget(logoTarget);

            ElementLoader.PreDrawAll(_spriteBatch, GraphicsDevice);

            string brush = ActiveBrush is TestBrush ? "Red pen" : "Eraser";
            
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.DrawString(font, $"Brush size : {Brush.BrushSize}", Vector2.Zero, Color.White);
            _spriteBatch.DrawString(font, $"Brush : {brush}", new Vector2(0, 30), Color.White);
            _spriteBatch.DrawString(font, $"Q : Red pen \nW : Eraser\nE : Big square", new Vector2(0, 60), Color.White);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(Canvas);
            //GraphicsDevice.Clear(Color.Transparent);
            ActiveBrush.MainDraw(_spriteBatch, GraphicsDevice);
            ElementLoader.PostDrawAll(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(final);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(logoTarget, Vector2.Zero, Color.White);
            _spriteBatch.Draw(Canvas, Vector2.Zero, Color.White);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(final, Vector2.Zero, Color.White);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}