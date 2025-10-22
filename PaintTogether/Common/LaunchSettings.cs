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
    /// Initalises Main.cs launch properties such as <see cref="Main.Resolution"/>
    /// </summary>
    public static class LaunchSettings
    {
        /// <summary>
        /// Initalises Main.cs launch properties such as <see cref="Main.Resolution"/>
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
        }
        
        private static void SetResolution(JsonElement root)
        {
            string x = root.GetProperty("Resolution").GetString();

            string[] strings = Regex.Split(x, @"[xX:|,\*\-]"); // Regex fucking sucks
            Main.Resolution = new Point(int.Parse(strings[0]), int.Parse(strings[1]));
        }
    }
}