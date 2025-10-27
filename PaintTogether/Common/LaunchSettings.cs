using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Common
{
    /// <summary>
    /// Initalises Main.cs launch properties such as <see cref="Main.CanvasResolution"/>
    /// </summary>
    public static class LaunchSettings
    {
        /// <summary>
        /// Initalises Main.cs launch properties such as <see cref="Main.CanvasResolution"/>
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(CommonKeys.LaunchSettingsFilePath))
            {
                throw new FileNotFoundException($"Could not find {CommonKeys.LaunchSettingsFilePath}");
            }
            string json = File.ReadAllText(CommonKeys.LaunchSettingsFilePath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            SetResolution(root);
            SetSavePath(root);
        }

        private static void SetResolution(JsonElement root)
        {
            string x = root.GetProperty("Resolution").GetString();

            // Look for any common seperators: [10x10] [10X10] [10:10] [10|10] [10,10] [10\10] [10*10] [10-10]
            string[] strings = Regex.Split(x, @"[xX:|,\*\-]"); 

            if (int.TryParse(strings[0], out int r1) && int.TryParse(strings[1], out int r2))
            {
                Main.CanvasResolution = new Point(r1, r2);
            }
            else
            {
                throw new Exception("Could not parse user specified resolution");
            }
        }
        
        private static void SetSavePath(JsonElement root)
        {
            Main.SaveFolderPath = root.GetProperty("SaveFolderPath").GetString();
        }
    }
}