using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.DataTypes;

namespace PaintTogether.Common.Utilities
{
    public static class CommonKeys
    {
        public const string LaunchSettingsFile = "LaunchConfig.json";

        /// <summary>
        /// Directory most files will go to. Log files, ect
        /// </summary>
        //public static readonly String MainDirectory = "/home/Glasta/Projects/PaintTogether/PaintTogether";
        public static readonly String MainDirectory = Directory.GetCurrentDirectory();

        public static readonly string LaunchSettingsFilePath = Path.Combine(MainDirectory, LaunchSettingsFile);
        
        private static Texture2D _dummyTexture;

        public static Texture2D DummyTexture
        {
            get
            {
                if (_dummyTexture == null)
                {
                    _dummyTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                    _dummyTexture.SetData([Color.Transparent]);
                }
                return _dummyTexture;
            }
        }

        private static Texture2D _whitePixel;
        public static Texture2D WhitePixel
        {
            get
            {
                if (_whitePixel == null)
                {
                    _whitePixel = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                    _whitePixel.SetData([Color.White]);
                }
                return _whitePixel;
            }
        }

    }
}