using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using PaintTogetherServer.Common.Utilities;

namespace PaintTogetherServer.Common.SvLogger
{
    /// <summary>
    /// Logging utilty for PaintTogether server program.
    /// </summary>
    public static class SvLogger
    {
        /// <summary>
        /// Path to the folder for log files
        /// </summary>
        public static string LogDirectory { get; private set; }

        /// <summary>
        /// Path to the PaintTogetherServer.log
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

        /// <summary>
        /// Enables extremely detailed logging. Can bloat log file.
        /// </summary>
        public static bool VerboseLogging = false;
        // TODO: disable versbose logging when finished

        
        public static void Init()
        {
            // Set path strings
            LogDirectory = Path.Combine(CommonKeys.MainDirectory, "Logs");
            LogFilePath = Path.Combine(LogDirectory, "PaintTogetherServer.log");

            // Make folder for logs, THEN make log files
            Directory.CreateDirectory(LogDirectory);
            LogFile = File.CreateText(LogFilePath);
            LogFile.AutoFlush = true;

            // Automatically log any errors before crashing
            AppDomain.CurrentDomain.UnhandledException += LogFatalError;
            AppDomain.CurrentDomain.FirstChanceException += LogAnyError;

            LogStartupInfo();
        }



        public static void Unload()
        {
            LogInfo($"Shutting down.");
            LogFile.Close();
            LogFile.Dispose();

            AppDomain.CurrentDomain.UnhandledException -= LogFatalError;
        }

        private static void LogStartupInfo()
        {
            LogInfo($"Starting PaintTogether Server v{LoggableData.ServerVersionInfo()}");
            LogInfo($"Log date : {DateTime.Now:dd/MM/yyyy}");
            LogInfo(LoggableData.RuntimeInfo());
            LogInfo($"CPU : {Environment.ProcessorCount} processors");
            LogInfo($"Executable : {Environment.ProcessPath}");
            LogInfo($"Working directory : {Path.GetFullPath(Directory.GetCurrentDirectory())}");
            LogInfo($"Process ID : {Environment.ProcessId}, Process memory usage : {(Process.GetCurrentProcess().PrivateMemorySize64 / 1048576d):f2}MB, Process priority : {Process.GetCurrentProcess().PriorityClass}");
        }

        /// <summary>
        /// Allows you to manually write some information to the log file
        /// </summary>
        public static void LogInfo(object args, bool verbose = false)
        {
            if (verbose && !VerboseLogging)
            {
                return;
            }
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
        /// Automatically-called method writes to log file when ANY error occurs, before it is even potentially handled
        /// </summary>
        private static void LogAnyError(object? sender, FirstChanceExceptionEventArgs e)
        {
            LogFile.WriteLine($"[{LogTime}] [ERROR] {e.Exception}");
        }

        /// <summary>
        /// Automatically-called method writes to the log file when any unhandled exception occurs
        /// </summary>
        public static void LogFatalError(object args, UnhandledExceptionEventArgs e)
        {
            LogFile.WriteLine($"[{LogTime}] [FATAL] {e.ExceptionObject}");
        }


    }
}