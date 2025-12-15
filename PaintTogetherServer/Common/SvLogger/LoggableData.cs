using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PaintTogetherServer.Common.SvLogger
{
    public static class LoggableData
    {
        /// <summary>
        /// Gets the version number of this PaintTogetherServer build. <br/>
        /// Eg 0.0.0.0
        /// </summary>
        public static string ServerVersionInfo()
        {
            var asm = Assembly.GetExecutingAssembly();
            var info = asm.GetName().Version;
            return info.ToString();
        }

        internal enum OperatingSystemType
        {
            Windows,
            Linux,
            MacOS,
            Unknown
        }

        // HOW is this the best way
        internal readonly static OperatingSystemType OS =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? OperatingSystemType.Windows
          : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? OperatingSystemType.Linux
          : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OperatingSystemType.MacOS
          : OperatingSystemType.Unknown;


        /// <summary>
        /// "Running on ARCHITECTURE OS using DOTNET"<br/>
        /// ex : "Running on X64 Linux using .NET 8.0.20
        /// </summary>
        internal static string RuntimeInfo()
        {
            return $"Running on {RuntimeInformation.ProcessArchitecture} {OS} using {RuntimeInformation.FrameworkDescription}";
        }
    }
}