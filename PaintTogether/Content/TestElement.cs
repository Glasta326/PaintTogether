using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;
using PaintTogether.Core.LoadSystem;

namespace PaintTogether.Content
{
    public class TestElement : Element
    {
        private Texture2D texture;
        private Vector2 offset;
        public override void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            texture = contentManager.Load<Texture2D>("Textures/logo");
        }

        public override void Update()
        {
            offset = MathUtils.RandomVector(0f, 3f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            Color c = Color.White;
            spriteBatch.Draw(texture, Vector2.Zero + offset, c);
            return false;
        }
    }
}