using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using PaintTogetherServer.Common.SvLogger;
using PaintTogetherServer.Core.ActionHistory;

namespace PaintTogetherServer.Core
{
    public class WorkerThread
    {
        public static List<WorkerThread> Workers = new List<WorkerThread>();

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

        // Ok so this whole thing needs a rewrite
        // basically, we need to change how the mouse data is sent
        // the new idea is:
        // the client will store the past second's worth of mouse position data.
        // then every second, it sends all that buffered data to the server
        // the server forwards that as per usual,
        // then each client reads each position in the recieved mouse data, and updates position in that stack
        // because we can "look into the future" because we have one second's worth of data, we can smoothly interpolate the mouse position across each position recieved
        // this is so much better performacne wise as i believe data transfer is better at doing fewer bigger things than many smaller things
        // apparently we also dont need to .flush

        // ALSO
        // i believe that is happening, like the issue with WAY too many packets:
        // the forearch task workqueue is working as i expect,
        // but when the threads are iterating through the clients, they end up writing to the same network stream
        // so we need thread-safe network streams.
        // proabbly create a lock for each PaintClient stream 
        // locks make threads WAIT untill the lock is removed so no data is lost dw future glasta pookie
        // Note: this might be unnicesary because we don't really care if two threads are writing data to the same stream?
        // we'll see

        private void Loop()
        {
            foreach (var task in WorkQueue.GetConsumingEnumerable())
            {
                // Log the event before doing anything else
                // Type 0 is registered to mouse movement, which we ignore as its super spammy and not worth storing
                if (task.Type != 0)
                {
                    EventReplay.AddAction(task);
                }

                foreach (PaintClient pc in Program.Clients.Values)
                {
                    // Obviously dont send data back to the sender
                    if (pc.ID == task.OwnerID)
                    {
                        continue;
                    }

                    // It hurts, but otherwise two threads can write to the same stream at the same time and garble all the data
                    // We are still geting multithreading performance though, just less
                    // think about it with 2 threads, if thread 1 and thread 2 both need to send out a packet:
                    // thread 1 is working on client a
                    // thread 2 is waiting for thread 1
                    // thread 1 is done, thread 2 now orks on client a while thread1 works on client b
                    // thread 2 is finished with client a, but at roughly the same time, thread 1 just finishes on client b and moves on
                    // so we still kind of align our threads and get performance gains
                    lock (pc.streamLock)
                    {
                        using var clientWriter = new BinaryWriter(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);

                        clientWriter.Write(task.OwnerID);
                        clientWriter.Write(task.Type);
                        clientWriter.Write(task.Length);
                        clientWriter.Write(task.Data.Span);
                    }


                    SvLogger.LogInfo($"Thread: [{myID}] Sent packet from [{task.OwnerID}] to [{pc.ID}] Containing [{task.Length}] bytes of data", true);
                }


                Thread.Sleep(1);
            }
        }

    }
}