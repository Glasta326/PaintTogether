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

        // Ok so this is fucked
        // im guessing each thread is somehow reading the same stuff from the queue
        // i need to find out how to multi-threadedly read from a externally updated queue
        // i know its unnessicary but im commited
        private void Loop()
        {
            foreach (var task in WorkQueue.GetConsumingEnumerable())
            {
                foreach (PaintClient pc in Program.Clients.Values.ToArray())
                {
                    using var writer = new BinaryWriter(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);
                    if (pc.ID != task.OwnerID)
                    {
                        continue;
                    }
                    writer.Write(task.Data);
                    writer.Flush();
                    SvLogger.LogInfo($"Thread: [{myID}] handled packet for user [{task.OwnerID}] aka [{Program.Clients[task.OwnerID].UserName}]");
                }


                Thread.Sleep(1);
            }
        }

    }
}