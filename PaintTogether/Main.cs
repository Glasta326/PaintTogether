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
        public static Texture2D logo;

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            Element.LoadAssetsAll(GraphicsDevice, Content);

            Canvas = new RenderTarget2D(GraphicsDevice, 800, 600);
            logo = Content.Load<Texture2D>("Textures/Logo");
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
            
            GraphicsDevice.SetRenderTarget(Canvas);
            _spriteBatch.Begin();
            _spriteBatch.Draw(logo, MouseUtils.MousePosVector(), Color.White);
            _spriteBatch.Draw(logo, MouseUtils.MousePosVector() + new Vector2(0, 100f * MathF.Sin(GlobalTimeWrappedHourly)), Color.Red);
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            
            
            
            
            
            _spriteBatch.Begin();
            _spriteBatch.Draw(Canvas, Vector2.Zero, null, Color.White, MathF.Sin(GlobalTimeWrappedHourly) * 0.2f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(logo, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();
            
            base.Draw(gameTime);
        }
    }
}