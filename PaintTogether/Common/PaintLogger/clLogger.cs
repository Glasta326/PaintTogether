using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PaintTogether.Common.DataTypes;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Common.PaintLogger
{
    /// <summary>
    /// Logging utility for PaintTogether.
    /// </summary>
    public static class clLogger // named "clLogger" purely because it is easy to type from my hand's resting position and sounds like "ClientLogger"
    {
        /// <summary>
        /// Path to the folder for log files
        /// </summary>
        public static string LogDirectory { get; private set; }

        /// <summary>
        /// Path to the PaintTogether.log
        /// </summary>
        public static string LogFilePath { get; private set; }

        /// <summary>
        /// Direct access to the stream for the client log file. <br/>
        /// Can be used to write to the file
        /// </summary>
        public static StreamWriter LogFile { get; private set; }

        /// <summary>
        /// Time formatted for log entries
        /// </summary>
        public static string LogTime => $"{DateTime.Now:HH:mm:ss.fff}";

        public static void Init()
        {
            // Set path strings
            LogDirectory = Path.Combine(CommonKeys.MainDirectory, "Logs");
            LogFilePath = Path.Combine(LogDirectory, "PaintTogether.log");

            // Make folder for logs, THEN make log files
            Directory.CreateDirectory(LogDirectory);
            LogFile = File.CreateText(LogFilePath);
            LogFile.AutoFlush = true;

            // Automatically log any errors before crashing
            AppDomain.CurrentDomain.UnhandledException += LogError;

            LogStartupInfo();
        }

        private static void LogStartupInfo()
        {
            LogInfo($"Starting PaintTogether Client v{LoggableData.ClientVersionInfo()}");
            LogInfo($"Log date : {DateTime.Now:dd/MM/yyyy}");
            LogInfo(LoggableData.RuntimeInfo());
            LogInfo($"CPU : {Environment.ProcessorCount} processors");
            LogInfo($"Executable : {Environment.ProcessPath}");
            LogInfo($"Working directory : {Path.GetFullPath(Directory.GetCurrentDirectory())}");
            LogInfo($"Process ID : {Environment.ProcessId}, Process memory usage : {(Process.GetCurrentProcess().PrivateMemorySize64 / 1048576d):f2}MB, Process priority : {Process.GetCurrentProcess().PriorityClass}");

        }


        public static void Unload()
        {
            LogFile.Close();
            LogFile.Dispose();

            AppDomain.CurrentDomain.UnhandledException -= LogError;
        }

        /// <summary>
        /// Allows you to manually write some information to the log file
        /// </summary>
        public static void LogInfo(object args)
        {
            LogFile.WriteLine($"[{LogTime}] [INFO] {args}");
            Console.WriteLine($"[{LogTime}] [INFO] {args}");
        }

        /// <summary>
        /// Allows you to manually write some error information to the log file, such as when an unexpected backup edge-case may executes
        /// </summary>
        public static void LogWarning(object args)
        {
            LogFile.WriteLine($"[{LogTime}] [WARN] {args}");
            Console.WriteLine($"[{LogTime}] [WARN] {args}");
        }   

        /// <summary>
        /// Automatically-called method writes to the log file when any unhandled exception occurs
        /// </summary>
        public static void LogError(object args, UnhandledExceptionEventArgs e)
        {
            LogFile.WriteLine($"[{LogTime}] [ERROR] {e.ExceptionObject}");
        }   
    }
}