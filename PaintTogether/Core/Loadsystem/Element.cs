





using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PaintTogether.Core.LoadSystem
{
    public abstract class Element
    {
        public static List<Element> ProgramElements = new List<Element>();

        public static void InitaliseRegistry()
        {
            var elementTypes = typeof(Element).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Element)) && !t.IsAbstract);

            foreach (var type in elementTypes)
            {
                if (Activator.CreateInstance(type) is Element instance)
                {
                    ProgramElements.Add(instance);
                }
            }
        }
        
        /// <summary>
        /// Load any non-asset resources here
        /// </summary>
        public virtual void Load() {}
        public static void LoadAll()
        {
            foreach (var e in ProgramElements)
                e.Load();
        }
        
        /// <summary>
        /// Safley unloads anything loaded in <see cref="Load"/>
        /// </summary>
        public virtual void Unload() {}
        public static void UnLoadAll()
        {
            foreach (var e in ProgramElements)
                e.Unload();
        }

        /// <summary>
        /// Load asset resources here <br/>
        /// Things like texture2d, font, ect.
        /// </summary>
        public virtual void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) {}
        public static void LoadAssetsAll(GraphicsDevice gd, ContentManager cm)
        {
            foreach (var e in ProgramElements)
                e.LoadAssets(gd, cm);
        }
        
        /// <summary>
        /// Called right at the start of the main update loop
        /// </summary>
        public virtual void PreUpdate() {}
        public static void PreUpdateAll()
        {
            foreach (var e in ProgramElements)
                e.PreUpdate();
        }
        
        public virtual void Update() {}
        public static void UpdateAll()
        {
            foreach (var e in ProgramElements)
                e.Update();
        }


        /// <summary>
        /// Used to draw things behind this element, or modify how the element is drawn <br/>
        /// Return false to prevent default drawing logic
        /// </summary>
        public virtual bool PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            return true;
        }
        public static void PreDrawAll(SpriteBatch sb, GraphicsDevice gd)
        {
            foreach (var e in ProgramElements)
                e.PreDraw(sb, gd);
        }
        
        /// <summary>
        /// Used to draw things infront of this element
        /// </summary>
        public virtual void PostDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) {}
        public static void PostDrawAll(SpriteBatch sb, GraphicsDevice gd)
        {
            foreach (var e in ProgramElements)
                e.PostDraw(sb, gd);
        }
    }
}