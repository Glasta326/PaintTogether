using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using PaintTogetherServer.Common.SvLogger;
using PaintTogetherServer.Core.ActionHistory;
using PaintTogetherServer.Core.UserRegistry;

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

                // TODO: untested!
                // Type 255 is the catchup request
                // A client will send a packet of type 255 when joining,
                // and when this thread sees the packet it will write every entry in the EventReplay log to the packet's sender
                if (task.Type == 255)
                {
                    PaintUser target = Program.RegisteredUsers[task.OwnerID];
                    if (!target.IsConnected)
                    {
                        continue;
                    }
#pragma warning disable CS8602
                    // The lock here ensures no other threads can try to send data to this client while we catch them up
                    lock (target.Connection.streamLock)
                    {
                        target.DoNotSend = true; // The stream is locked already, but this prevents threads from wasting time getting stuck on the streamlock hopefully

                        // First specify how many Logged actions there are to catch up on
                        // otherwise the client has no way to know when we've stopped sending catchup packets and have started sending normal ones
                        int length = EventReplay.ActionHistory.Count;
                        target.Connection.Writer.Write(length);

                        foreach (LoggedAction item in EventReplay.ActionHistory)
                        {
                            InfoPacket data = item.Packet;
                            target.Connection.Writer.Write(data.OwnerID);
                            target.Connection.Writer.Write(data.Type);
                            target.Connection.Writer.Write(data.Length);
                            target.Connection.Writer.Write(data.Data.Span);
                        }

                        target.DoNotSend = false;
                    }
#pragma warning restore CS8602
                }

                // TODO:
                // Eventually i want to full redo this system, having it so each PaintUser has it's own send loop
                // So the clientConnection will have its own blockingCollection of packets, and it's own networkstream
                // and every user also get's its own task that sends packets
                // worker threads will just enqueue the packets from the main queue into the right user
                // so instead of dequeing from the WorkQueue, writing and sending
                // THey now take the dequeued packet, and add that packet to every user's queue (except the sender)
                // then the user's sending task will dequeue packets from the blockingCOllection and send them
                foreach (PaintUser user in Program.RegisteredUsers._UsersById.Values)
                {
                    // Dont send packets to anyone not connected or back to the person who sent this packet lol
                    if (user.DoNotSend || !user.IsConnected)//|| task.OwnerID == user.ClientID)
                    {
                        continue;
                    }
#pragma warning disable CS8602 // <- User must be considered "connected", so the pc connection cannot be null

                    // It hurts, but otherwise two threads can write to the same stream at the same time and garble all the data
                    // We are still geting multithreading performance though, just less
                    // think about it with 2 threads, if thread 1 and thread 2 both need to send out a packet:
                    // thread 1 is working on client a
                    // thread 2 is waiting for thread 1
                    // thread 1 is done, thread 2 now orks on client a while thread1 works on client b
                    // thread 2 is finished with client a, but at roughly the same time, thread 1 just finishes on client b and moves on
                    // so we still kind of align our threads and get performance gains
                    lock (user.Connection.streamLock)
                    {
                        user.Connection.Writer.Write(task.OwnerID);
                        user.Connection.Writer.Write(task.Type);
                        user.Connection.Writer.Write(task.Length);
                        user.Connection.Writer.Write(task.Data.Span);
                    }


                    SvLogger.LogInfo($"Thread: [{myID}] Sent packet from [{task.OwnerID}] to [{user.ClientID}] Containing [{task.Length}] bytes of data", true);
                }
#pragma warning restore CS8602
            }
        }

    }
}