using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PaintTogetherServer.Common.SvLogger;

namespace PaintTogetherServer
{
    public static partial class Program
    {
        [ThreadStatic] private static Random _rand;
        public static Random rand
        {
            get { return _rand ??= new Random(); }
            set { _rand = value; }
        }

        [ThreadStatic] private static Stopwatch _sw;
        public static Stopwatch sw
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
        /// The version of this program. Clients must be on the same version or they will be disconnected <br/>
        /// </summary>
        public static string VERSION = LoggableData.ServerVersionInfo();

        /// <summary>
        /// The number of worker threads the server will create<br/>
        /// Defaults to -1, which will have the program automatically decide the amount
        /// </summary>
        public static int ThreadCount = -1;

        /// <summary>
        /// The port that the <see cref="Program.Listener"/> runs on
        /// </summary>
        public static ushort ListenerPort = 12504;

        /// <summary>
        /// Maximum amount of users that can be registered on this server
        /// </summary>
        public static int MaxUsers = byte.MaxValue;
    }   
}