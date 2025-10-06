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

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            Element.LoadAssetsAll(GraphicsDevice, Content);

            Canvas = new RenderTarget2D(GraphicsDevice, 800, 600);
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
            
            _spriteBatch.Begin();

            Element.PreDrawAll(_spriteBatch, GraphicsDevice);


            _spriteBatch.Draw(Canvas, new Rectangle(0, 0, 800, 600), Color.White);

            Element.PostDrawAll(_spriteBatch, GraphicsDevice);
            
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}