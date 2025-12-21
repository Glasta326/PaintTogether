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
        
        
        private void Loop()
        {
            foreach (var task in WorkQueue.GetConsumingEnumerable())
            {
                
                foreach (PaintClient pc in Program.Clients.Values.ToArray())
                {
                    // Obviously dont send data back to the sender
                    if (pc.ID == task.OwnerID)
                    {
                        //continue;
                    }
                    using var writer = new BinaryWriter(pc.Stream, System.Text.Encoding.UTF8, leaveOpen: true);
                    writer.Write(task.Data);
                    writer.Flush();
                    SvLogger.LogInfo($"Thread: [{myID}] Sent packet from [{task.OwnerID}] aka [{Program.Clients[task.OwnerID].UserName}] to [{pc.ID}] aka [{pc.UserName}]");
                }


                Thread.Sleep(1);
            }
        }

    }
}