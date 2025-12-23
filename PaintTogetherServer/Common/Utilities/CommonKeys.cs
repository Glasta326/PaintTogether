using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Common.Utilities
{
    public static class CommonKeys
    {
        /// <summary>
        /// Directory most files will go to. Log files, ect
        /// </summary>
        public static readonly String MainDirectory = "/home/Glasta/Projects/PaintTogether/PaintTogetherServer";
        //public static readonly String MainDirectory = Directory.GetCurrentDirectory();

        public enum ServerPacketTypes
        {
            // Forced user disconnect reasons
            ServerConnectionLimitReached = 1,
            BadUID = 2,
            VersionMismatch = 3,

            // Server requests
            RequestUsername = 11,

            // Server broadcasts
            NewUserConnecting = 21,
            ExistingUserConnecting = 22,
            UserDisconnecting = 23
        }


        /// <summary>
        /// All packets created by the server and sent out will have a user ID of 255
        /// </summary>
        public const byte ServerPacketID = 255;
    }
}