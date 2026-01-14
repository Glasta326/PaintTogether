using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogether.Core.Networking.Registry
{
    // ok so, some notes
    // I think we're just going to change things so packet type is a string.
    // it sucks, but there's no good way to give each tool a unique id without writing it all manually from what i can gather
    // supposedly the registry system isnt consistent.
    // packets will look like [byte owner][string type][length][data]
    // so for example, [255][PaintTogether.Content.Applicators.LineTool][58][255,233,...]
    // that way assigning id's is super easy
    // oh well
    public static class NetRegistry
    {
        private static Dictionary<ushort, INetApplicable> NetApplicables = new Dictionary<ushort, INetApplicable>();


        /// <summary>
        /// Registers an element which implements the INetApplicable interface into the registry
        /// </summary>
        /// <param name="element"></param>
        /// <exception cref="Exception"></exception>
        public static void Register(INetApplicable element)
        {
            ushort id = GetNetID(element.GetType());

            if (NetApplicables.ContainsKey(id))
            {
                throw new Exception($"Duplicate element ID entry: {id}");
            }
            NetApplicables[id] = element;

            id++;
        }

        public static bool TryGet(byte id, out INetApplicable element) => NetApplicables.TryGetValue(id, out element);


        private static ushort GetNetID(Type type)
        {
            unchecked
            {
                ushort hash = 0;
                foreach (char c in type.FullName)
                    hash = (ushort)(hash * 31 + c);
                return hash;
            }
        }
    }
}