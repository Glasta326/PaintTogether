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
        public static BlockingCollection<InfoPacket> WorkQueue = new(new ConcurrentQueue<InfoPacket>());

        public Thread myThread;

        private int myID;

        public WorkerThread(int id)
        {
            myID = id;
            myThread = new Thread(Init);
            myThread.IsBackground = true;
            myThread.Start();
        }

        private void Init()
        {
            

            
            Loop();
        }

        private void Loop()
        {
            foreach (var task in WorkQueue.GetConsumingEnumerable())
            {
                foreach (var pc in Program.Clients)
                {
                    using var writer = new BinaryWriter(pc.stream, System.Text.Encoding.UTF8, leaveOpen: true);
                    if (pc.name == task.owner)
                    {
                        continue;
                    }
                    foreach (var d in task.data)
                    {
                        // once again you can't just write objects
                        writer.Write(d);
                    }
                    writer.Flush();

                    if (task.data[0] is Point p)
                    {
                        writer.Write(p.X);
                        writer.Write(p.Y);
                        writer.Flush();
                        Console.WriteLine($"{task},{pc.name},{myID}");
                    }                    
                }


                Thread.Sleep(1);
            }
        }

    }
}