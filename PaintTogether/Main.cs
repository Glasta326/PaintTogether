using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.Utilities;
using PaintTogether.Core;
using PaintTogether.Core.LoadSystem;

namespace PaintTogether
{
    public class Main : Game
    {
        public static Main instance;
        
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        [ThreadStatic] private static Random _rand;

        public static Random rand
        {
            get { return _rand ??= new Random(); }
            set { _rand = value; }
        }

        public static float GlobalTimeWrappedHourly;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Element.InitaliseRegistry();
            
            Element.LoadAll();

            base.Initialize();
        }


        public static RenderTarget2D Canvas;
        public static RenderTarget2D logoTarget;
        public static Texture2D logo;

        

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        

            Element.LoadAssetsAll(GraphicsDevice, Content);
            logoTarget = new RenderTarget2D(GraphicsDevice, Window.ClientBounds.Width, Window.ClientBounds.Height);
            Canvas = new RenderTarget2D(GraphicsDevice,  Window.ClientBounds.Width, Window.ClientBounds.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            logo = Content.Load<Texture2D>("Textures/proxy-image");
            
        }

        protected override void Update(GameTime gameTime)
        {
            Update_Inner(gameTime);
            
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            Element.PreUpdateAll();
            
            
            
            Element.UpdateAll();
            base.Update(gameTime);
        }

        private void Update_Inner(GameTime gameTime)
        {
            MouseUtils.State = Mouse.GetState(); // We do this and just read from state when getting mouse info so we arent requesting to get the state a zillion times
            MouseUtils.MoveHistory.Add(MouseUtils.State.Position);
            MouseUtils.ScrollHistory.Add(MouseUtils.State.ScrollWheelValue); // Push the new scroll value to the scroll history so scrollDelta is accurate
            GlobalTimeWrappedHourly = (float)(gameTime.TotalGameTime.TotalSeconds % 3600.0);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.SetRenderTarget(logoTarget);

            Element.PreDrawAll(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(Canvas);
            //GraphicsDevice.Clear(Color.Transparent);

            Element.PostDrawAll(_spriteBatch, GraphicsDevice);

            GraphicsDevice.SetRenderTarget(null);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            _spriteBatch.Draw(logoTarget, Vector2.Zero, Color.White);
            _spriteBatch.Draw(Canvas, Vector2.Zero, Color.White);
            _spriteBatch.End();

            
            base.Draw(gameTime);
        }
    }
}