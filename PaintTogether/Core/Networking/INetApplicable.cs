using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.Networking
{
    public interface INetApplicable
    {
        public void RecieveNetCall(byte owner, BinaryReader reader);

        public void SendNetCall(BinaryWriter writer);
    }
}