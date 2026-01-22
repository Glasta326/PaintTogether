using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.DataTypes;

namespace PaintTogether.Common.Utilities
{
    public static class CommonKeys
    {
        public const string LaunchSettingsFile = "LaunchConfig.json";

        /// <summary>
        /// Directory most files will go to. Log files, ect
        /// </summary>
        //public static readonly String MainDirectory = "/home/Glasta/Projects/PaintTogether/PaintTogether";
        public static readonly String MainDirectory = Directory.GetCurrentDirectory();

        public static readonly string LaunchSettingsFilePath = Path.Combine(MainDirectory, LaunchSettingsFile);

        private static Texture2D _dummyTexture;

        public static Texture2D DummyTexture
        {
            get
            {
                if (_dummyTexture == null)
                {
                    _dummyTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                    _dummyTexture.SetData([Color.Transparent]);
                }
                return _dummyTexture;
            }
        }

        private static Texture2D _whitePixel;
        public static Texture2D WhitePixel
        {
            get
            {
                if (_whitePixel == null)
                {
                    _whitePixel = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                    _whitePixel.SetData([Color.White]);
                }
                return _whitePixel;
            }
        }

        public enum ServerPacketTypes
        {
            #region Forced user disconnect reasons

            /// <summary>
            /// There are too many users registered on this server.
            /// </summary>
            RejectServerConnectionLimitReached = 1,

            /// <summary>
            /// The GUID sent by the client was badly formatted somehow
            /// </summary>
            RejectBadGUID = 2,

            /// <summary>
            /// The version information sent by the client is not the same version as the server is running on
            /// </summary>
            RejectVersionMismatch = 3,

            /// <summary>
            /// The client tried to join on a user GUID that is already currently connected
            /// </summary>
            RejectUserAlreadyConnected = 4,

            /// <summary>
            /// Something unknown went wrong and the user could not be registered
            /// </summary>
            RejectUserUnknown = 5,

            #endregion

            #region Server requests

            /// <summary>
            /// The server is asking the client to send the username string it wants to use
            /// </summary>
            RequestUsername = 11,

            #endregion

            #region Server broadcasts

            /// <summary>
            /// A user has connected to the server
            /// </summary>
            AnnounceUserConnecting = 21,

            /// <summary>
            /// A user who is currently connected has just disconnected
            /// </summary>
            AnnounceUserDisconnecting = 22,

            /// <summary>
            /// The server has been shut down and all users need to disconnect.
            /// </summary>
            AnnounceServerClosing = 23,

            #endregion

            #region Server whispers

            /// <summary>
            /// Informs a newly-joined client what networkID they are
            /// </summary>
            WhisperInformClientID = 31,

            /// <summary>
            /// Informs a client that catchuping up is about to begin, and how many packets there will be to catch up with
            /// </summary>
            WhisperInformCatchupBegin = 32

            #endregion
        }

        /// <summary>
        /// Specific packet types clients can send which require special logic and handling <br/>
        /// Only ranges from 0-255 but still in an array to match the standard packet format
        /// </summary>
        public static class SpecialPacketTypes
        {
            public static string MouseMovement = "MouseMovement";

            public static string CatchupRequest = "CatchupRequest";
        }

        /// <summary>
        /// All packets created by the server and sent out will have a user ID of 255
        /// </summary>
        public const byte ServerPacketID = 255;
    }
}