using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using PaintTogether.Common.PaintLogger;
using PaintTogether.Common.Utilities;
using PaintTogether.Content.PaintCanvas;
using PaintTogether.Core.Networking;

namespace PaintTogether.Common
{
    /// <summary>
    /// Initalises Main.cs launch properties such as <see cref="Main.CanvasResolution"/>
    /// </summary>
    public static class LaunchSettings
    {
        #region  Defaults

        private static readonly Point DefaultResolution = new(1200, 720);
        private static readonly string DefaultSavePath = Path.Combine(CommonKeys.MainDirectory, "Saves");
        private static readonly bool DefaultVerboseLogging = true;
        private static readonly Guid DefaultGuid = Guid.NewGuid();

        #endregion

        /// <summary>
        /// Initalises Main.cs launch properties such as <see cref="Main.CanvasResolution"/>
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(CommonKeys.LaunchSettingsFilePath))
            {
                clLogger.LogWarning($"Could not find {CommonKeys.LaunchSettingsFilePath}. Creating default file in it's place.");
                MakeDefaultFile();
            }
            clLogger.LogInfo($"Reading launch settings file from {CommonKeys.LaunchSettingsFilePath}");

            string json = File.ReadAllText(CommonKeys.LaunchSettingsFilePath);
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            SetResolution(root);
            SetSavePath(root);
            SetLogState(root);
            SetGUID(root);
        }

        private static void SetResolution(JsonElement root)
        {
            string x = root.GetProperty("Resolution").GetString();

            // Look for any common seperators: [10x10] [10X10] [10:10] [10|10] [10,10] [10\10] [10*10] [10-10]
            string[] strings = Regex.Split(x, @"[xX:|,\*\-]");

            if (int.TryParse(strings[0], out int r1) && int.TryParse(strings[1], out int r2) && r1 >= 0 && r2 >= 0)
            {
                Canvas.Resolution = new Point(r1, r2);
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
            clLogger.LogInfo($"Saved file output will be at {Main.SaveFolderPath}");
            if (!Directory.Exists(Main.SaveFolderPath))
            {
                Directory.CreateDirectory(Main.SaveFolderPath);
                clLogger.LogWarning($"Save file directory did not exist! Created save folder directory at {Main.SaveFolderPath}");
            }
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

        private static void SetGUID(JsonElement root)
        {
            string x = root.GetProperty("GUID").GetString();
            Guid guid = new Guid(x);
            NetSorter.MyGuid = guid;
            clLogger.LogInfo($"Set GUID as {guid}");
        }


        // If a config file can't be found
        private static void MakeDefaultFile()
        {
            var json = new JsonObject
            {
                ["Resolution"] = $"{DefaultResolution.X}:{DefaultResolution.Y}",
                ["SaveFolderPath"] = DefaultSavePath,
                ["VerboseLogging"] = DefaultVerboseLogging,
                ["GUID"] = DefaultGuid.ToString()
            };

            File.WriteAllText(CommonKeys.LaunchSettingsFilePath, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            clLogger.LogInfo($"Created new default settings file at {CommonKeys.LaunchSettingsFilePath}");
        }
    }
}