





using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.PaintLogger;

namespace PaintTogether.Core
{
    /// <summary>
    /// Base element that almost all things that happen in-program are composed of. UI, brushes, ect
    /// </summary>
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
                    clLogger.LogInfo($"Registered program element : {instance.ToString()}");
                }
            }
            clLogger.LogInfo($"{ProgramElements.Count} elements registered");
        }

        /// <summary>
        /// Can be used to sort element types to be loaded before others if it is needed. <br/>
        /// Higher values mean higher priority. Priority 10 is loaded before priority 5. <br/>
        /// Defaults to 0.
        /// </summary>
        public virtual int LoadPriority => 0;

        public static T Get<T>() where T : Element
        {
            return ProgramElements.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Load any non-asset resources here
        /// </summary>
        public virtual void Load() {}
        public static void LoadAll()
        {
            foreach (var e in ProgramElements.OrderByDescending(l => l.LoadPriority))
            {
                e.Load();
                clLogger.LogInfo($"Loaded resources for : {e} with priority {e.LoadPriority}");    
            }
            
        }
        
        /// <summary>
        /// Safley unloads anything loaded in <see cref="Load"/>
        /// </summary>
        public virtual void Unload() {}
        public static void UnLoadAll()
        {
            foreach (var e in ProgramElements)
            {
                e.Unload();
                clLogger.LogInfo($"Unloaded resources for : {e} with priority {e.LoadPriority}");    
            }
        }

        /// <summary>
        /// Load asset resources here <br/>
        /// Things like texture2d, font, ect.
        /// </summary>
        public virtual void LoadAssets(GraphicsDevice graphicsDevice, ContentManager contentManager) {}
        public static void LoadAssetsAll(GraphicsDevice gd, ContentManager cm)
        {
            foreach (var e in ProgramElements)
            {
                e.LoadAssets(gd, cm);
                clLogger.LogInfo($"Loaded assets for : {e} with priority {e.LoadPriority}");    
            }
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
        public virtual bool PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice) { return true; }
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