using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PaintTogether.Common.PaintLogger;
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
            clLogger.LogInfo($"Reading launch settings file from {CommonKeys.LaunchSettingsFilePath}");

            string json = File.ReadAllText(CommonKeys.LaunchSettingsFilePath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            SetResolution(root);
            SetSavePath(root);
            SetLogState(root);
        }

        private static void SetResolution(JsonElement root)
        {
            string x = root.GetProperty("Resolution").GetString();

            // Look for any common seperators: [10x10] [10X10] [10:10] [10|10] [10,10] [10\10] [10*10] [10-10]
            string[] strings = Regex.Split(x, @"[xX:|,\*\-]");

            if (int.TryParse(strings[0], out int r1) && int.TryParse(strings[1], out int r2) && r1 >= 0 && r2 >= 0)
            {
                Main.CanvasResolution = new Point(r1, r2);
                clLogger.LogInfo($"Set canvas resolution to {r1} x {r2}");
                return;
            }
            else
            {
                throw new Exception($"Could not parse user specified resolution");
            }
        }

        private static void SetSavePath(JsonElement root)
        {
            Main.SaveFolderPath = root.GetProperty("SaveFolderPath").GetString();
            clLogger.LogInfo($"Set saved files output file path to {Main.SaveFolderPath}");
        }

        private static void SetLogState(JsonElement root)
        {
            string x = root.GetProperty("VerboseLogging").GetString();
            string[] trueKwrds = ["true", "yes", "1"];
            string[] falseKwrds = ["false", "no", "0"];

            if (trueKwrds.Contains(x.ToLower()))
            {
                clLogger.VerboseLogging = true;
            }
            else if (falseKwrds.Contains(x.ToLower()))
            {
                clLogger.VerboseLogging = false;
            }
            else
            {
                clLogger.LogWarning("Could not parse VerboseLogging setting.");

                // If something goes so wrong that the setting isnt misspelt or something silly, then we probably want this enabled
                clLogger.VerboseLogging = true; 
            }
        }
    }
}