using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.Utilities;
using PaintTogether.Core;

namespace PaintTogether
{
    public class Main : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        [ThreadStatic] private static Random _rand;

        public static Random rand
        {
            get { return _rand ??= new Random(); }
            set { _rand = value; }
        }

        public static CanvasData Canvas;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Assets";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }


        private Texture2D output;
        private ShiftRegister<Point> reg;
        private Effect shader;
        private Texture2D logo;
        private SpriteFont font;

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Canvas = new CanvasData(800, 800);
            output = new Texture2D(GraphicsDevice, (int)Canvas.Width, (int)Canvas.Height);
            reg = new ShiftRegister<Point>(3);
            
            shader = Content.Load<Effect>("Shaders/test2");
            logo = Content.Load<Texture2D>("Textures/Logo");
            font = Content.Load<SpriteFont>("Fonts/TestFont");
        }

        private float timer = 0f;
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            // Ok lets make a seperate project and port this all over
            // Namely one where sln contains many csproj because i want the server and client in the same solution dammit
            // FUCK i might have to do that shared library thing



            reg.Add(MouseUtils.MousePosPoint());

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Vector2 mousepos = MouseUtils.MousePosVector();

                //DrawUtils.DrawLine(oldMousePos, oldMousePos, out _, out _);
                DrawUtils.DrawLine(reg[0], reg[1], out _, out _);
                // Maybe draw line between old mouse pos and new?

            }




            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                Canvas.ClearCanvas();
            }

            output.SetData(Canvas.Data);

            timer += 0.0166666667f;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            _spriteBatch.Begin();
            
            _spriteBatch.Draw(output, Vector2.Zero, Color.White);
            _spriteBatch.End();
            

            _spriteBatch.Begin(effect: shader);

            float opacity = (MathF.Sin(timer) + 1f) / 2f;
            shader.Parameters["Saturation"].SetValue(0.1f + opacity * 0.9f);
            _spriteBatch.DrawString(font, "Press R to reset canvas", Vector2.Zero, Color.White);
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}