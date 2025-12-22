using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Content.Applicators.Brushes;
using PaintTogether.Core;

namespace PaintTogether
{
    public partial class Main
    {

        #region Initalised elements

        public static Main instance;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        [ThreadStatic] private static Random _rand;
        public static Random rand
        {
            get { return _rand ??= new Random(); }
            set { _rand = value; }
        }

        [ThreadStatic] private static Stopwatch _sw;
        public static Stopwatch stopWatch
        {
            get
            {
                return _sw ??= new Stopwatch();
            }
            private set
            {
                _sw = value;
            }
        }

        /// <summary>
        /// Folder where all saved image files will go to.
        /// </summary>
        public static string SaveFolderPath;

        #endregion

        #region Active elements

        /// <summary>
        /// Counts up once every second and wraps every 3600 seconds
        /// </summary>
        public static float GlobalTimeWrappedHourly;
        
        /// <summary>
        /// Counts up once per frame and wraps every 60 frames
        /// </summary>
        public static byte GlobalFramesWrappedSecond;

        /// <summary>
        /// Current brush type being held by the user
        /// </summary>
        public static Element ActiveBrush;

        #endregion
    }
}