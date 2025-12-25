using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaintTogetherServer.Core.UserRegistry
{
    public class PaintUsers
    {
        // Both dictionaries point to the same values in memory, so this isnt two dicts with two seperate instances in each
        // Classes are essentialy references to an object in memory
        // structs are actualy instances in memory
        // Apparently

        /// <summary>
        /// Dictionary indexed by network id. Primarily for indexing over
        /// </summary>
        public ConcurrentDictionary<uint, PaintUser> _UsersById = new ConcurrentDictionary<uint, PaintUser>(-1, Program.MaxUsers);

        private ConcurrentDictionary<Guid, PaintUser> _UsersByGuid = new ConcurrentDictionary<Guid, PaintUser>(-1, Program.MaxUsers);

        public PaintUser this[Guid guid] => _UsersByGuid[guid]; // "cannot have indexers in static class WHY???????"

        public PaintUser this[uint id] => _UsersById[id];



        // Returns the number of items in the Registry
        public int Count => _UsersById.Count;

        public bool TryAdd(PaintUser _user)
        {
            // Premature optimization. If somehow the dicts end up desynced
            if (Count >= Program.MaxUsers)
            {
                return false;
            }

            byte c = 0;
            if (_UsersByGuid.TryAdd(_user.UserID, _user))
            {
                c++;
            }
            if (_UsersById.TryAdd(_user.ClientID, _user))
            {
                c++;
            }
            return c == 2; // Returns true only when both were succesfully added
        }

        public bool Remove(PaintUser _user)
        {
            byte c = 0;
            if (_UsersByGuid.Remove(_user.UserID, out _))
            {
                c++;
            }
            if (_UsersById.Remove(_user.ClientID, out _))
            {
                c++;
            }
            return c == 2;
        }

        public bool TryGetValue(Guid _guid, out PaintUser? _user)
        {
            return _UsersByGuid.TryGetValue(_guid, out _user);
        }

        public bool TryGetValue(uint _id, out PaintUser? _user)
        {
            return _UsersById.TryGetValue(_id, out _user);
        }

        public void Unload()
        {
            foreach (var item in _UsersByGuid)
            {
                if (item.Value.Connection is not null)
                {
                    item.Value.Connection.tcp.Close();
                }
            }
            _UsersByGuid.Clear();
            _UsersById.Clear();
        }
    }
}