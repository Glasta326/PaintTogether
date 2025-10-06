using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PaintTogether.Common.Utilities;
using PaintTogether.Core.LoadSystem;

namespace PaintTogether.Content
{
    public class Brush : Element
    {
        private Point Position;
        private Effect BrushShader;

        private int _brushSize;
        private int BrushSize
        {
            get
            {
                return _brushSize;
            }
            set
            {
                _brushSize = MathHelper.Clamp(value, 0, int.MaxValue);
            }
        }
        
        public override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            BrushShader = contentManager.Load<Effect>("Shaders/test2");
        }

        private bool draw = false;
        public override void Update()
        {
            draw = false;
            if (MouseUtils.LeftClick == ButtonState.Pressed)
            {
                draw = true;
            }
            Position = MouseUtils.MousePosPoint();
            BrushSize += MouseUtils.ScrollDelta; // Scroll to change brush size
            Console.WriteLine(BrushSize);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (!draw)
            {
                return true;
            }
            graphicsDevice.SetRenderTarget(Main.Canvas);

            BrushShader.Parameters["BrushCenter"].SetValue(MouseUtils.MousePosNormalised());
            //BrushShader.Parameters["BrushRadius"].SetValue(BrushSize);
            BrushShader.Parameters["BrushColor"].SetValue(Color.Red.ToVector4());
            
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, BrushShader);
            spriteBatch.Draw(Main.Canvas, new Rectangle(0, 0, Main.Canvas.Width, Main.Canvas.Height), Color.White);
            
            spriteBatch.End();
            spriteBatch.Begin();

            graphicsDevice.SetRenderTarget(null);

            
            return false;
        }
    }
}