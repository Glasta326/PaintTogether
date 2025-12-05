using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using PaintTogetherServer.Common.SvLogger;

namespace PaintTogetherServer.Core
{
    public class WorkerThread
    {
        public static BlockingCollection<Point> WorkQueue = new(new ConcurrentQueue<Point>());

        public Thread myThread;

        private int myID;

        public WorkerThread(int id)
        {
            myID = id;
            myThread = new Thread(Loop);
            myThread.IsBackground = true;
            myThread.Start();
        }

        private void Loop()
        {
            foreach (var task in WorkQueue.GetConsumingEnumerable())
            {
                foreach (var pc in Program.Clients)
                {
                    using var writer = new BinaryWriter(pc.stream, System.Text.Encoding.UTF8, leaveOpen: true);
                    writer.Write(task.X);
                    writer.Write(task.Y);
                    writer.Flush();

                    Console.WriteLine($"{task},{pc.name},{myID}");
                }


                Thread.Sleep(1);
            }
        }

    }
}