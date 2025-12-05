using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
        /// The number of worker threads the server will create<br/>
        /// Defaults to -1, which will have the program automatically decide the amount
        /// </summary>
        public static int ThreadCount = -1;

        /// <summary>
        /// The port that the <see cref="Program.Listener"/> runs on
        /// </summary>
        public static ushort ListenerPort = 12504;
    }   
}