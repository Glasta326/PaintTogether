using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.DataTypes;

namespace PaintTogether.Core.Loadsystem
{
    public class ILoadableRegistry
    {
        public static List<ILoadable> LoadableTypes = new();

        public static void Initialize()
        {
            var loadableTypes = typeof(ILoadable).Assembly.GetTypes()
                .Where(t => typeof(ILoadable).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

            foreach (var type in loadableTypes)
            {
                if (Activator.CreateInstance(type) is ILoadable instance)
                {
                    LoadableTypes.Add(instance);
                }
            }
        }

        /// <summary>
        /// Returns a loaded <see cref="ILoadable"/> instance
        /// </summary>
        public static T Get<T>() where T : ILoadable
        {
            return LoadableTypes.OfType<T>().FirstOrDefault();
        }

        public static void LoadAll()
        {
            foreach (var t in LoadableTypes)
                t.Load();
        }

        public static void LoadAllAssets(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            foreach (var t in LoadableTypes)
                t.LoadAssets(graphicsDevice, contentManager);
        }
        
        public static void UnLoadAll()
        {
            foreach (var t in LoadableTypes)
                t.Unload();
        }
    }
}