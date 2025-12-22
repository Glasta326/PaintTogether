using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core.ActionHistory
{
    public static class EventReplay
    {
        private static readonly BlockingCollection<InfoPacket> InfoQueue = new BlockingCollection<InfoPacket>();

        public static readonly List<LoggedAction> ActionHistory = new List<LoggedAction>();

        /// <summary>
        /// Only ever modified by the ingestThread.
        /// </summary>
        public static ulong ActionCounter = 0;

        /// <summary>
        /// Rough estimate based on average size of an <see cref="InfoPacket"/>
        /// </summary>
        public static long MemEstimate => 88 * ActionHistory.Count;

        public static void Init()
        {
            Thread ingestThread = new Thread(ConsumeQueue);
            ingestThread.IsBackground = true;
            ingestThread.Start();
        }

        // Worker threads add to the blockingCollection, which is then consumed by this thread to add each entry into the true actionHistory queue
        private static void ConsumeQueue()
        {
            foreach (var data in InfoQueue.GetConsumingEnumerable())
            {
                ActionHistory.Add(new LoggedAction(ActionCounter, DateTime.Now, data, Program.Clients[data.OwnerID]));
                ActionCounter++;
            }
        }

        /// <summary>
        /// Adds a new <see cref="LoggedAction"/> to the event history based on a recieved packet
        /// </summary>
        public static void AddAction(InfoPacket data)
        {
            InfoQueue.Add(data);
        }
    }
}