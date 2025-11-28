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
using PaintTogether.Content.Applicators.Brushes;
using PaintTogether.Content.UI;
using PaintTogether.Core;
using PaintTogether.Content;
using PaintTogether.Content.PaintCanvas;
using PaintTogether.Core.UndoSystem;
using PaintTogether.Content.Applicators.Tools;
using PaintTogether.Content.Applicators.ClickTools;

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

            Canvas.Init(GraphicsDevice);

            Element.InitaliseRegistry();
            Element.LoadAll();


            base.Initialize();
        }

        public static RenderTarget2D logoTarget;

        public static RenderTarget2D UITarget;


        public static RenderTarget2D final;
        public static Texture2D logo;

        public static SpriteFont font;


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Element.LoadAssetsAll(GraphicsDevice, Content);

            logoTarget = new RenderTarget2D(GraphicsDevice, Canvas.Resolution.X, Canvas.Resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            UITarget = new RenderTarget2D(GraphicsDevice, Canvas.Resolution.X, Canvas.Resolution.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
            final = new RenderTarget2D(GraphicsDevice, Canvas.Resolution.X, Canvas.Resolution.Y);
            logo = Content.Load<Texture2D>("Textures/proxy-image");
            font = Content.Load<SpriteFont>("Fonts/TestFont");

            // TODO : remove this this is just for demonstrating the layers working
            Canvas.Layers.AddBasicLayer();


        }

        protected override void OnExiting(object sender, ExitingEventArgs args)
        {
            Element.UnLoadAll();
            clLogger.Unload();
        }

        static Element b;

        protected override void Update(GameTime gameTime)
        {
            if (ActiveBrush is null)
            {
                ActiveBrush = Element.Get<PenBrush>();
            }

            if (t is null)
            {
                //t = Element.Get<TestTool>();
            }

            if (_b is null)
            {
                //_b = Element.Get<PenBrush>();
            }
            if (_t is null)
            {
                _t = Element.Get<testClick>();
            }
            // I need to get input sorted fucking desperatly
            if (!ColorSelector.isFocused)
            {
                //t = Element.Get<TestTool>();
            }

            /*
            if (b is Brush d)
            {
                d.BrushShader;
            }
            if (b is Tool t)
            {
                t.ToolShader;
            }
            */

            if (!ColorSelector.isFocused)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.D1))
                {
                    ActiveBrush = Element.Get<PenBrush>();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.D2))
                {
                    ActiveBrush = Element.Get<EraserBrush>();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.R))
                {
                    ActiveBrush = Element.Get<TestTool>();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.F))
                {
                    ActiveBrush = Element.Get<TestLineTool>();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.D3))
                {
                    ActiveBrush = Element.Get<testClick>();
                }
                // color picker is controlled inside its own class for now

                if (Keyboard.GetState().IsKeyDown(Keys.F1))
                {
                    Canvas.Layers.ActiveLayerIndex = 0;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.F2))
                {
                    Canvas.Layers.ActiveLayerIndex = 1;
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


            Canvas.Layers.ActiveLayer.GetData<Color>(0, region, c, 0, 4);

        }

        private void Update_Inner(GameTime gameTime)
        {
            MouseData.State = Mouse.GetState(); // We do this and just read from state when getting mouse info so we arent requesting to get the state a zillion times
            MouseData.MoveHistory.Push(MouseData.MousePosPoint());
            MouseData.ScrollHistory.Push(MouseData.State.ScrollWheelValue); // Push the new scroll value to the scroll history so scrollDelta is accurate
            MouseData.ClickHistory.Push(MouseData.LeftClick == ButtonState.Pressed);
            KeyboardData.state = Keyboard.GetState();
            KeyboardData.KeyboardHistory.Push(Keyboard.GetState());
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

            HistoryManager.Update();
        }

        public static DragTool t;
        public static Brush _b;
        public static ClickTool _t;
        private void UpdateBrush()
        {
            if (ActiveBrush is not null)
            {
                ActiveBrush.Update();
            }
            //if (_b is not null)
            //{
            //    _b.Update();
            //}
            
        }

        protected override void Draw(GameTime gameTime)
        {
            // Manually re-clear the preview layer before anything tries to draw to it
            Canvas.ResetPreviewLayer(GraphicsDevice);

            #region Drawing stuff to rendertargets

            GraphicsDevice.SetRenderTarget(Canvas.Layers.ActiveLayer);
            //GraphicsDevice.Clear(Color.White);
            //ActiveBrush.MainDraw(_spriteBatch, GraphicsDevice);

            if (ActiveBrush is Brush b)
            {
                b.MainDraw(_spriteBatch,GraphicsDevice);
            }
            if (ActiveBrush is DragTool d)
            {
                d.MainDraw(_spriteBatch,GraphicsDevice);
            }
            if (ActiveBrush is ClickTool c)
            {
                c.MainDraw(_spriteBatch,GraphicsDevice);
            }

            GraphicsDevice.SetRenderTarget(UITarget);
            GraphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.DrawString(font, $"Brush size : {Canvas.Camera.Position}", Vector2.Zero, Color.White);
            _spriteBatch.End();
            Element.PostDrawAll(_spriteBatch, GraphicsDevice);

            
            if (ActiveBrush is Brush B)
            {
                B.UIDraw(_spriteBatch,GraphicsDevice);
            }
            if (ActiveBrush is DragTool D)
            {
                //D.UIDraw(_spriteBatch,GraphicsDevice);
            }
            if (ActiveBrush is ClickTool C)
            {
                C.UIDraw(_spriteBatch,GraphicsDevice);
            }

            #endregion

            GraphicsDevice.SetRenderTarget(Canvas.PreviewLayer);
            _spriteBatch.Begin();
            //_spriteBatch.Draw(logo, Vector2.Zero, Color.White);
            _spriteBatch.End();

            //t.MainDraw(_spriteBatch,GraphicsDevice);
            //_b.MainDraw(_spriteBatch, GraphicsDevice);
            //_t.MainDraw(_spriteBatch,GraphicsDevice);
            
            HistoryManager.Draw();

            /*
            if (HistoryManager.CommandHistory.Count > 0 && Keyboard.GetState().IsKeyDown(Keys.K))
            {
                HistoryManager.CommandHistory.Last().Undo();
                HistoryManager.CommandHistory.RemoveAt(HistoryManager.CommandHistory.Count - 1);
            }
            */
            #region Actually drawing stuff to the output

            GraphicsDevice.SetRenderTarget(null);

            Canvas.Draw(GraphicsDevice, _spriteBatch, null);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(UITarget, Vector2.Zero, Color.White);
            _spriteBatch.End();

            #endregion

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