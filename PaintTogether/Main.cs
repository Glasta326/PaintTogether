using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.Brushes;
using PaintTogether.Content.UI;
using PaintTogether.Core;
using PaintTogether.Content.PaintCanvas;

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
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            clLogger.Init();

            LaunchSettings.Load();

            Element.InitaliseRegistry();
            Element.LoadAll();

            
            base.Initialize();
        }

        public static RenderTarget2D Canvas;
        public static RenderTarget2D logoTarget;

        public static RenderTarget2D UITarget;


        public static RenderTarget2D final;
        public static Texture2D logo;

        public static SpriteFont font;


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Element.LoadAssetsAll(GraphicsDevice, Content);

            logoTarget = new RenderTarget2D(GraphicsDevice, CanvasResolution.X, CanvasResolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            Canvas = new RenderTarget2D(GraphicsDevice, CanvasResolution.X, CanvasResolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            UITarget = new RenderTarget2D(GraphicsDevice, CanvasResolution.X, CanvasResolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            final = new RenderTarget2D(GraphicsDevice, CanvasResolution.X, CanvasResolution.Y);
            logo = Content.Load<Texture2D>("Textures/proxy-image");
            font = Content.Load<SpriteFont>("Fonts/TestFont");

            GraphicsDevice.SetRenderTarget(Canvas);
            GraphicsDevice.Clear(Color.White);
        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            Element.UnLoadAll();
            clLogger.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (ActiveBrush is null)
            {
                ActiveBrush = Element.Get<PenBrush>();
            }

            if (!ColorSelector.isFocused)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Q))
                {
                    ActiveBrush = Element.Get<PenBrush>();
                }

                if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    ActiveBrush = Element.Get<Test2Brush>();
                }

                if (Keyboard.GetState().IsKeyDown(Keys.E))
                {
                    ActiveBrush = Element.Get<TestBrush3>();
                }
            }

            Update_Inner(gameTime);

            Element.PreUpdateAll();

            UpdateBrush();

            Element.UpdateAll();

            base.Update(gameTime);

            Color[] c = new Color[10];
            Rectangle region = new Rectangle(0, 0, 2, 2);

            GraphicsDevice.SetRenderTarget(null);


            Canvas.GetData<Color>(0, region, c, 0, 4);
            
        }

        private void Update_Inner(GameTime gameTime)
        {
            MouseData.State = Mouse.GetState(); // We do this and just read from state when getting mouse info so we arent requesting to get the state a zillion times
            MouseData.MoveHistory.Add(MouseData.MousePosPoint());
            MouseData.ScrollHistory.Add(MouseData.State.ScrollWheelValue); // Push the new scroll value to the scroll history so scrollDelta is accurate
            GlobalTimeWrappedHourly = (float)(gameTime.TotalGameTime.TotalSeconds % 3600.0);

            if (MouseData.RightClick == ButtonState.Pressed)
            {
                // we do -MoveDelta,
                // if you think about it relativley, inverting the movement of the camera position is essentially moving the canvas with the camera as the reference frame.
                PaintTogether.Content.PaintCanvas.Canvas.Camera.Position += MouseData.MoveDelta.ToVector2() / PaintTogether.Content.PaintCanvas.Canvas.Camera.Zoom;
            
                // Potentially make a CameraPosition class with custom methods for moving and overloaded operators and whatnot
                // Porbably a good idea
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                PaintTogether.Content.PaintCanvas.Canvas.Camera.ZoomToPosition(1.1f, MouseData.MousePosVector());
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                PaintTogether.Content.PaintCanvas.Canvas.Camera.ZoomToPosition(0.9f, MouseData.MousePosVector());
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                // dividing by cameraZoom makes it so the movement is always 1 pixel on the actual physical screen
                PaintTogether.Content.PaintCanvas.Canvas.Camera.Position += new Vector2(-1, 0) / PaintTogether.Content.PaintCanvas.Canvas.Camera.Zoom;
            }
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
            GraphicsDevice.SetRenderTarget(Canvas);
            //GraphicsDevice.Clear(Color.White);
            ActiveBrush.MainDraw(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(UITarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.DrawString(font, $"Brush size : {PaintTogether.Content.PaintCanvas.Canvas.Camera.Position}", Vector2.Zero, Color.White);
            _spriteBatch.End();
            Element.PostDrawAll(_spriteBatch, GraphicsDevice);
            ActiveBrush.UiDraw(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: PaintTogether.Content.PaintCanvas.Canvas.CanvasTransform(), samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(Canvas, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(UITarget, Vector2.Zero, Color.White);
            _spriteBatch.End();


            /*
            GraphicsDevice.SetRenderTarget(logoTarget);
            GraphicsDevice.Clear(new Color(15, 15, 15));
            Element.PreDrawAll(_spriteBatch, GraphicsDevice);
            string brush = ActiveBrush is TestBrush ? "Red pen" : "Eraser";
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.DrawString(font, $"Brush size : {Brush.BrushSize}", Vector2.Zero, Color.White);
            _spriteBatch.DrawString(font, $"Brush : {brush}", new Vector2(0, 30), Color.White);
            _spriteBatch.DrawString(font, $"Q : Red pen \nW : Eraser\nE : Big square", new Vector2(0, 60), Color.White);
            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(Canvas);
            GraphicsDevice.Clear(Color.White);
            ActiveBrush.MainDraw(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(UITarget);
            GraphicsDevice.Clear(Color.Transparent);
            Element.PostDrawAll(_spriteBatch, GraphicsDevice);
            ActiveBrush.UiDraw(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(final);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(logoTarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, transformMatrix: GetCanvasTransform(), samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(Canvas, Vector2.Zero, Color.White);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(UITarget, Vector2.Zero, Color.White);
            _spriteBatch.End();




            // TODO:
            // Ok so just "scale the whole canvas to the screen" isnt really good enough
            // you can see in the pain that the canvas always stays the same size, making the window smaller just makes you see less of the canvas,
            // and you use bars to move around when zoomed in
            // also the UI always stays the same size
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(final, new Rectangle(0,0,(int)WindowData.WindowSize.X,(int)WindowData.WindowSize.Y), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);

            */
        }
    }
}